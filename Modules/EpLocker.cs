using HarmonyLib;
using nel;

namespace Kaleidoscopic.Modules;

[Module("兴奋度锁在 0")]
public class EpLocker {
    [HarmonyPostfix]
    [HarmonyPatch(typeof(EpManager), "run")]
    public static void EpManager_run(EpManager __instance) {
        __instance.ep = 0;
    }
}