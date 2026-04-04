using HarmonyLib;
using nel;

namespace Kaleidoscopic.Modules;

[Module("随地打开仓库")]
public static class EnderChest {
    [HarmonyPatch(typeof(NelM2DBase), "canAccesableToHouseInventory")]
    [HarmonyPostfix]
    public static void Postfix(NelM2DBase __instance, ref bool __result) {
        __result = true;
    }
}