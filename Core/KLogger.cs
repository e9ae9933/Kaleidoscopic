global using static Kaleidoscopic.KLogger;
using System;
using System.Diagnostics;
using System.Reflection;

namespace Kaleidoscopic;

public class KLogger {
    public static void info(params object[] obj) {
        KaleidoscopicPlugin.LOGGER.LogInfo(getPrefix() + string.Join(" ", obj));
    }
    public static void warning(params object[] obj) {
        KaleidoscopicPlugin.LOGGER.LogWarning(getPrefix() + string.Join(" ", obj));
    }
    public static void error(params object[] obj) {
        KaleidoscopicPlugin.LOGGER.LogError(getPrefix() + string.Join(" ", obj));
    }
    public static void debug(params object[] obj) {
        KaleidoscopicPlugin.LOGGER.LogDebug(getPrefix() + string.Join(" ", obj));
    }

    private static string getPrefix() {
        MethodBase callerMethod = new StackFrame(2).GetMethod();
        Type callerType = callerMethod?.DeclaringType;
        if (callerType is not null) {
            // string ns = callerType.Namespace??"";
            // int i = ns.LastIndexOf('.');
            // string substr = ns.Substring(i >= 0 ? i : 0);
            return $"[{callerType.Name}/{callerMethod.Name}] ";
        }
        return "[Unknown method] ";
    }
}