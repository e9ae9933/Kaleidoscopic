using HarmonyLib;
using nel;

namespace Kaleidoscopic.modules;

public class ItemStacker {
    [HarmonyPatch(typeof(ItemStorage), "getItemStockable")]
    [HarmonyPostfix]
    public static void Postfix(ItemStorage __instance, ref int __result) {
        if (__result >= 2) {
            __result = 9999;
        } else {
            __result = 5;
        }
    }
}