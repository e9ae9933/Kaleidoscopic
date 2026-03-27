global using static Kaleidoscopic.KLogger;

namespace Kaleidoscopic;

public class KLogger {
    public static void info(params object[] obj) {
        KaleidoscopicPlugin.LOGGER.LogInfo(string.Join(" ", obj));
    }
    public static void warning(params object[] obj) {
        KaleidoscopicPlugin.LOGGER.LogWarning(string.Join(" ", obj));
    }
    public static void error(params object[] obj) {
        KaleidoscopicPlugin.LOGGER.LogError(string.Join(" ", obj));
    }
    public static void debug(params object[] obj) {
        KaleidoscopicPlugin.LOGGER.LogDebug(string.Join(" ", obj));
    }
}