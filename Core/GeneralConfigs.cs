using BepInEx.Configuration;

namespace Kaleidoscopic.Core;

public static class GeneralConfigs {
    public static ConfigEntry<bool> multiEnabled;
    public static ConfigEntry<string> multiServer;
    public static ConfigEntry<string> multiName;
    public static ConfigEntry<bool> multiRetry;
    public static ConfigEntry<int> multiDelay;
    public static ConfigEntry<bool> multiAllowPVP;
    public static ConfigEntry<bool> coreUsePlane0;
    public static void bind(ConfigFile config) {
        multiEnabled = config.Bind("多人联机", "启用多人联机", true);
        multiServer = config.Bind("多人联机", "服务器连接地址与端口", "sync.aliceincradle.org:25561");
        multiRetry = config.Bind("多人联机", "断线自动重连", true);
        multiName = config.Bind("多人联机", "多人名称", $"按 F1 或 Fn+F1 打开菜单，修改名称");
        multiAllowPVP = config.Bind("多人联机", "启用PVP", true);
        coreUsePlane0 = config.Bind("通用", "使用完整码表", true, "如果你的显存不足2G可能需要关闭这个。重启生效");
    }
}