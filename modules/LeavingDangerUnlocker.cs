using HarmonyLib;
using nel;
using XX;

namespace Kaleidoscopic.modules;

public class LeavingDangerUnlocker {
    private static readonly int[] lvs = [
        0, 21, 41,
        0 + 64, 21 + 64, 41 + 64,
        128 + 0, 160 + 0, 160 + 9
    ];

    [HarmonyPatch(typeof(UiDangerLevelInitBox), "val2nightLevel")]
    [HarmonyPostfix]
    public static void pf1(int val, ref int __result) {
        if (val >= 0 && val < 9) {
            __result = lvs[val];
        }
    }
    [HarmonyPatch(typeof(UiDangerLevelInitBox), "fnDescConvertSlider")]
    [HarmonyPrefix]
    public static void pf2(UiDangerLevelInitBox __instance, STB Stb, ref bool __runOriginal) {
        __runOriginal = false;
        int num = Stb.NmI(0);
        Stb.Clear();
        int lv = __instance.val2nightLevel(num);
        int day = (lv >> 4) + 1;
        int time = lv % 16;
        if (time >= 9) {
            Stb.AddTxA("Depert_from_night").TxRpl(day).TxRpl(lv);
        } else if (time >= 5) {
            Stb.AddTxA("Depert_from_evening").TxRpl(day).TxRpl(lv);
        } else {
            Stb.AddTxA("Depert_from_noon").TxRpl(day).TxRpl(lv);
        }
    }
}