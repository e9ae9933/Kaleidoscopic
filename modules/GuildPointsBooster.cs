using HarmonyLib;
using nel;

namespace Kaleidoscopic.modules;

public class GuildPointsBooster {
    [HarmonyPatch(typeof(GQCategInfo), "reward_gp")] // yes it is a shit-like method.
    [HarmonyPostfix]
    public static void Postfix(GQCategInfo __instance, ref int __result) {
        __result *= 5;
    }
}