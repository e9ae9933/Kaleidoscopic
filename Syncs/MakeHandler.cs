using System;
using HarmonyLib;
using Kaleidoscopic.Syncs.Packets;
using m2d;
using nel;
using UnityEngine;

namespace Kaleidoscopic.Syncs;

[Module("伤害数字显示")]
public static class MakeHandler {
    public static volatile bool suppressThis = false;
    private static long cnt = 0;
    [HarmonyPatch(typeof(M2DmgCounterContainer), nameof(M2DmgCounterContainer.Make))]
    [HarmonyPatch(typeof(M2DmgCounterContainer), nameof(M2DmgCounterContainer.MakeAbsorb))]
    static void Prefix(M2Attackable Mv, int delta_hp, int delta_mp, M2DmgCounterItem.DC et) {
        if (suppressThis || Mv == null) return;
        Dispatcher.send(new ClientBoundDmgCounterPacket {
            isPlayer = Mv is PR,
            x = Mv.x,
            y = Mv.y,
            dcInt = (int)et,
            damage = delta_hp,
            mpDamage = delta_mp,
        });
    }
    // .PHONY Process Prefix
    // Prefix: Process Postfix
    public static void Process(ServerBoundDmgCounterPacket p) {
        try {
            // info("process");
            suppressThis = true;
            GameObject go = new GameObject("Gunmu2_" + (++cnt));
            M2Gunmu ghost = go.AddComponent<M2Gunmu>();
            var pr = SceneGame.M2D?.PlayerNoel;
            var mp = pr?.Mp;
            if (mp == null || mp.Gob == null || pr.destructed) return;
            ghost.appear(mp);
            ghost.moveTo(p.original.x, p.original.y);
            int dcInt = p.original.dcInt;
            if (p.original.isPlayer)
                dcInt |= 1 << 24;
            mp.DmgCntCon.Make(ghost, p.original.damage, p.original.mpDamage, (M2DmgCounterItem.DC)dcInt, false);
            ghost.destruct();
        } catch (Exception e) {
            error(e);
        } finally {
            suppressThis = false;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(M2DmgCounterItem), nameof(M2DmgCounterItem.FineMv))]
    static void Postfix2(M2DmgCounterItem __instance) {
        int customFlag = 1 << 24;
        if (((int)__instance.et & customFlag) != 0) {
            __instance.is_pr = 1;
            __instance.et = (M2DmgCounterItem.DC)((int)__instance.et & ~customFlag);
        }
    }
}