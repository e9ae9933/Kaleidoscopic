using HarmonyLib;
using nel;

namespace Kaleidoscopic.Modules;

[Module("食物在战斗后不消耗")]
public static class StomachComsumeSuppresser {
    [HarmonyPatch(typeof(NelItemManager), "battleFinishProgress")]
    public static void NelItemManager_battleFinishProgress(NelItemManager __instance, ref int grade, bool gameover) {
        grade = 0;
    }
}