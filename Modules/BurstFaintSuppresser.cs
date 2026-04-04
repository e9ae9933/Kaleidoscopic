using HarmonyLib;
using m2d;
using nel;

namespace Kaleidoscopic.Modules;

[Module("圣光爆发不晕厥")]
public class BurstFaintSuppresser {
    // [HarmonyPostfix]
    // [HarmonyPatch(typeof(MDAT), nameof(MDAT.calcBurstFaintedRatio))]
    // public static void BurstFaintSuppresser_Postfix(ref float __result) {
    //     __result = 0f;
    // }
    [HarmonyPrefix]
    [HarmonyPatch(typeof(M2Ser), "Add")]
    public static void M2Ser_Add_Postfix(M2Ser __instance, ref M2SerItem __result, SER ser, ref bool __runOriginal) {
        if (ser == SER.BURST_TIRED && __instance.Mv is PR pr) {
            if (pr.getSkillManager().BurstSel.fainted_ratio < 1) //,)
                __runOriginal = false;
        }
    }
}