using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kaleidoscopic.Core;
using Kaleidoscopic.Syncs.Packets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kaleidoscopic.Syncs;

public static class Frontend {
    private static ClientWebSocket _ws;
    private static CancellationTokenSource _cts;
    
    private static bool _isReconnecting = false;
    
    // 标记是否为主动停止（默认设为 true，以便玩家中途开启配置时 Tick 能正确捕捉到）
    private static bool _intentionalStop = true; 

    // 动态从配置中读取 URL (支持热更改服务器地址)
    public static string Url => $"ws://{GeneralConfigs.multiServer.Value}";

    public static bool IsConnected => _ws != null && _ws.State == WebSocketState.Open;

    public static void Start() {
        if (!GeneralConfigs.multiEnabled.Value) {
            info("多人联机在配置中未启用，跳过连接。");
            _intentionalStop = true; // 确保状态正确，以便后续热更改可以开启
            return;
        }
        
        _intentionalStop = false;
        _ = ConnectInternal();
    }

    private static async Task ConnectInternal() {
        if (IsConnected) {
            warning("已经尝试连接到服务器！");
            return;
        }

        _ws = new ClientWebSocket();
        _cts = new CancellationTokenSource();

        try {
            info($"正在连接至 {Url}...");
            await _ws.ConnectAsync(new Uri(Url), _cts.Token);
            info("WebSocket 连接成功。");
            
            _isReconnecting = false; 
            _ = ReceiveLoop();
        } catch (Exception e) {
            error($"连接失败: {e.Message}");
            HandleDisconnect(); // 首次连接失败，直接进入无限重连逻辑
        }
    }

    public static async void Tick() {
        // 【热更改支持】：每帧检测配置项启用状态
        bool isEnabledConfig = GeneralConfigs.multiEnabled.Value;
        if (isEnabledConfig && _intentionalStop) {
            info("检测到配置热更改：多人联机已启用，正在启动...");
            Start();
            return; // 等待下一帧再处理队列
        } else if (!isEnabledConfig && !_intentionalStop) {
            info("检测到配置热更改：多人联机已禁用，正在断开...");
            Stop();
            return; // 直接中止发送
        }

        if (!IsConnected) return;

        while (Dispatcher.TryGetPacketToSend(out ClientBoundPacket packet)) {
            try {
                string json = JsonConvert.SerializeObject(packet);
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                await _ws.SendAsync(
                    new ArraySegment<byte>(bytes),
                    WebSocketMessageType.Text,
                    true,
                    _cts.Token
                );
            } catch (Exception e) {
                error($"发送错误: {e.Message}");
                HandleDisconnect(); 
                break; 
            }
        }
    }

    private static async Task ReceiveLoop() {
        byte[] buffer = new byte[8192];
        try {
            while (IsConnected && !_cts.Token.IsCancellationRequested) {
                WebSocketReceiveResult result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);

                if (result.MessageType == WebSocketMessageType.Close) {
                    info("服务器主动关闭连接。");
                    break;
                }

                string json = Encoding.UTF8.GetString(buffer, 0, result.Count);

                while (!result.EndOfMessage) {
                    result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);
                    json += Encoding.UTF8.GetString(buffer, 0, result.Count);
                }

                Dispatcher.received(json);
            }
        } catch (Exception e) {
            if (!_cts.Token.IsCancellationRequested)
                warning($"接收循环异常: {e.Message}");
        } finally {
            HandleDisconnect(); 
        }
    }

    // 核心重连控制流 (已修复只连一次的 Bug)
    private static async void HandleDisconnect() {
        if (_intentionalStop) return; 

        CleanUp(false);

        if (!GeneralConfigs.multiRetry.Value) {
            info("断线重连已在配置中禁用。");
            return;
        }

        // 闸门：防止同一时间多个断开异常触发多个重连循环
        if (_isReconnecting) return; 
        _isReconnecting = true;

        // 【重连无限循环】：只要没连上、没被关停配置，就一直尝试
        while (!_intentionalStop && GeneralConfigs.multiRetry.Value && !IsConnected) {
            warning("网络已断开，将在 5 秒后尝试重连...");
            await Task.Delay(5000);

            // 延迟后【热更改支持】：玩家可能在等待的这 5 秒内关了配置
            if (_intentionalStop || !GeneralConfigs.multiRetry.Value) {
                break;
            }

            try {
                info($"正在尝试重新连接至 {Url}...");
                _ws = new ClientWebSocket();
                _cts = new CancellationTokenSource();
                await _ws.ConnectAsync(new Uri(Url), _cts.Token);
                
                info("WebSocket 重连成功！");
                _ = ReceiveLoop();
                break; // 连上了，打破循环！
            } catch (Exception e) {
                error($"重连失败: {e.Message}");
                CleanUp(false); // 失败就彻底清理，然后继续下一次 while 循环
            }
        }

        // 循环结束（不论是连上了还是被关闭了），重置闸门
        _isReconnecting = false;
    }

    public static void Stop() {
        _intentionalStop = true;
        CleanUp(true);
        info("WebSocket 服务已手动停止。");
    }

    private static void CleanUp(bool normalClosure) {
        if (_ws == null) return;

        _cts?.Cancel();

        if (normalClosure && _ws.State == WebSocketState.Open) {
            try { 
                _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client stopping", CancellationToken.None); 
            } catch { }
        } else if (_ws.State != WebSocketState.Closed && _ws.State != WebSocketState.Aborted) {
            _ws.Abort(); 
        }

        _ws.Dispose();
        _ws = null;
    }
}

public static class Dispatcher {
    private static readonly ConcurrentQueue<ServerBoundPacket> sq = new();
    private static readonly ConcurrentQueue<ClientBoundPacket> cq = new();
    private static readonly Dictionary<string, Type> typeCache = new();

    static Dispatcher() {
        foreach (Type type in Assembly.GetExecutingAssembly().GetTypes()) {
            if (typeof(ServerBoundPacket).IsAssignableFrom(type) && !type.IsAbstract) {
                typeCache[type.Name] = type;
            }
        }
    }

    public static void received(string json) {
        try {
            JObject jo = JObject.Parse(json);
            string className = jo["clazz"]?.ToString();
            if (className != null && typeCache.TryGetValue(className, out Type targetType)) {
                ServerBoundPacket packet = (ServerBoundPacket)JsonConvert.DeserializeObject(json, targetType);
                if (packet != null) sq.Enqueue(packet);
            }
        } catch (Exception e) {
            error($"deserialize failed: {e.Message}");
        }
    }

    public static void send(ClientBoundPacket p) {
        cq.Enqueue(p);
    }

    public static bool TryGetPacketToSend(out ClientBoundPacket p) {
        return cq.TryDequeue(out p);
    }

    public static void handleAll() {
        while (sq.TryDequeue(out ServerBoundPacket packet)) {
            // debug("there are "+sq.Count+" packets still in sq");
            // debug("there are "+cq.Count+" packets still in cq");
            packet.Process();
        }
    }
}