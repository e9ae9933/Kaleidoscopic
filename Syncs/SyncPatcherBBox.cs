using System.Collections.Generic;
using HarmonyLib;
using JetBrains.Annotations;
using Kaleidoscopic.Core;
using Kaleidoscopic.Syncs.Packets;
using m2d;
using nel;

namespace Kaleidoscopic.Syncs;

[Module("碰撞箱发送")]
public class SyncPatcherBBox {
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Map2d), nameof(Map2d.run))]
    public static void something(Map2d __instance) {
        if (StateHolder.skip("syncpatcherbox")) return;
        Map2d mp = __instance;
        int n = mp.mover_count;
        List<BoundingBox> list = new();
        for (int i = 0; i < n; i++) {
            M2Mover mv = mp.AMov[i];
            if (mv is NelEnemy || (mv is PR) && GeneralConfigs.multiAllowPVP.Value) {
                if (mv is M2Attackable ma) {
                    list.Add(generateEnemyInfo(ma));
                }
            }
        }
        Dispatcher.send(new ClientBoundBboxPacket() {
            bboxes = list.ToArray()
        });
    }

    [CanBeNull]
    public static BoundingBox generateEnemyInfo(M2Attackable en) {
        float width = en.getColliderCreator()?.sizex??1;
        float height = en.getColliderCreator()?.sizey??1;
        var points = en.getColliderCreator()?.Cld?.points;
        float[] x = null, y = null;
        if (points != null) {
            x = new float[points.Length];
            y = new float[points.Length];
            for (int i = 0; i < points.Length; i++) {
                x[i] = points[i].x;
                y[i] = points[i].y;
            }
        }
        return new() {
            x = x, y = y,
            mapX = en.x, mapY = en.y,
            width = width, height = height,
            key = en.key, token = StateHolder.whoami
        };
    }
}