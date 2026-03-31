using HarmonyLib;
using m2d;
using nel;

namespace Kaleidoscopic.modules;

[Module("免疫着火状态")]
public static class BurnRemover {
    [HarmonyPostfix]
    [HarmonyPatch(typeof(M2Ser), "Add")]
    public static void M2Ser_Add_Postfix(M2Ser __instance, ref M2SerItem __result, m2d.SER ser) {
        if (ser == SER.BURNED && __instance.Mv is PR) {
            __instance.Cure(SER.BURNED);
        }
    }
}