using System.Collections.Generic;
using HarmonyLib;
using m2d;

namespace Kaleidoscopic.Syncs;

[Module("棍母滚木辊暮")]
public class GunmuFixer {
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Map2d), nameof(Map2d.close), [])]
    // [HarmonyPatch(typeof(Map2d), nameof(Map2d.close), [typeof(bool), typeof(bool)])]
    public static void Map2d_close_Prefix(Map2d __instance) {
        // List<M2Gunmu> list = new();
        for (int i = 0; i < __instance.mover_count; i++) {
            // if (__instance.AMov[i] is M2Gunmu gunmu) {
            //     list.Add(gunmu);
            // }
            var mv = __instance.AMov[i];
            if (ReferenceEquals(mv, null)) {
                info("????");
            } else if (mv == null) {
                info("它要么是真null，要么是底层的 GameObject 已经被 Destroy 掉了！");
            } else {
                info($"# {i} - {mv}");
                info($"{mv.gameObject}");
                info(mv.gameObject == null);
                info(mv.destructed);
            }
        }
        // foreach (M2Gunmu m2Gunmu in list) {
        //     m2Gunmu.destruct();
        //     m2Gunmu.Mp?.removeMover(m2Gunmu);
        // }
    }
}