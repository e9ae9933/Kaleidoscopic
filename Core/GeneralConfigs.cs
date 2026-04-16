using BepInEx.Configuration;

namespace Kaleidoscopic.Core;

public static class GeneralConfigs {
    public static ConfigEntry<bool> multiEnabled;
    public static ConfigEntry<string> multiServer;
    public static ConfigEntry<string> multiName;
    public static ConfigEntry<bool> multiRetry;
    public static void bind(ConfigFile config) {
        multiEnabled = config.Bind("多人联机", "启用多人联机", false);
        multiServer = config.Bind("多人联机", "服务器地址", "127.0.0.1:25560");
        multiRetry = config.Bind("多人联机", "断线重连", true);
        multiName = config.Bind("多人联机", "多人名称", $"Player{randInt(0, 99999):d5}");
    }
}