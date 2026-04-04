using HarmonyLib;
using nel;

namespace Kaleidoscopic.Modules;

[Module("减少 QTE 需要次数")]
public class GachaSuppresser {
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PrGachaItem), "activate", typeof(PrGachaItem.TYPE), typeof(int), typeof(bool), typeof(uint))]
    public static void activate(PrGachaItem __instance, PrGachaItem.TYPE _type, int need_count, bool do_not_fix_by_difficulty,
        uint _key_alloc_bit) {
        __instance.count0 = 1;
    }
}