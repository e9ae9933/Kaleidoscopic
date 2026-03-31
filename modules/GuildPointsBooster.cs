using HarmonyLib;
using nel;

namespace Kaleidoscopic.modules;

[Module("工会任务点变为 5 倍")]
public static class GuildPointsBooster {
    [HarmonyPatch(typeof(GQCategInfo), "reward_gp")] // yes it is a shit-like method.
    [HarmonyPostfix]
    public static void Postfix(GQCategInfo __instance, ref int __result) {
        __result *= 5;
    }
}