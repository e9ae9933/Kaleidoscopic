using HarmonyLib;
using nel;
using XX;

namespace Kaleidoscopic.modules;

[Module("解锁效果叠加")]
public static class StomachLinearer {
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Stomach), "addEffect", typeof(RCP.RPI_EFFECT), typeof(float), typeof(float))]
    private static void onAddEffect(Stomach __instance, ref bool __runOriginal,
        ref Stomach __result, RCP.RPI_EFFECT ef, float lvl01, float multiply) {
        __runOriginal = false;
        __result = __instance;
        if (ef == RCP.RPI_EFFECT.NONE)
            return;
        float v2 = X.IntR(X.MMX(-1f, lvl01, 1f) * 100f) / 100f * multiply;
        if (v2 >= 0.0) {
            float v1 = X.Get(__instance.OEffect, ef, 0.0f);
            if (__instance.ADuped != null && v1 > 0.0 && v1 >= (double)v2 && __instance.ADuped.IndexOf(ef) == -1)
                __instance.ADuped.Add(ef);
            // __instance.OEffect[ef] = X.Mn(X.Mx(v1, v2), 1f);
            __instance.OEffect[ef] = X.Mn(v1 + v2, 1f);
        } else
            __instance.OReduce[ef] = X.Get(__instance.OReduce, ef, 0.0f) - v2;
    }
}