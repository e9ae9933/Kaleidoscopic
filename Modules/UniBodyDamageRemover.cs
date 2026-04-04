using HarmonyLib;
using nel;

namespace Kaleidoscopic.Modules;

[Module("取消剑山碰撞伤害")]
public static class UniBodyDamageRemover {
    [HarmonyPostfix]
    [HarmonyPatch(typeof(NelNUni), MethodType.Constructor)]
    public static void UniBodyDamageRemoverPostfix(NelNUni __instance) {
        __instance.AtkTackleBody.hpdmg0 = 0;
    }
}