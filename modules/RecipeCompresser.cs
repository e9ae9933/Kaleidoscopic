using System;
using HarmonyLib;
using nel;

namespace Kaleidoscopic.modules;

[Module("降低食品饱腹度")]
public static class RecipeCompresser {
    [HarmonyPostfix]
    [HarmonyPatch(typeof(RCP.RecipeDish), "cost", MethodType.Getter)]
    private static void onGetCost(RCP.RecipeDish __instance, ref int __result) {
        __result = Math.Min(1, __result);
    }
}