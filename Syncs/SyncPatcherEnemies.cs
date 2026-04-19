using System.Collections.Generic;
using HarmonyLib;
using JetBrains.Annotations;
using Kaleidoscopic.Syncs.Packets;
using m2d;
using nel;
using PixelLiner;
using UnityEngine;
using XX;

namespace Kaleidoscopic.Syncs;

[Module("联机核心敌人模块")]
public class SyncPatcherEnemies {
    public static EnemyInfo[] enemyInfos;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Map2d), nameof(Map2d.run))]
    public static void something(Map2d __instance) {
        Map2d mp = __instance;
        int n = mp.mover_count;
        List<EnemyInfo> list = new();
        for (int i = 0; i < n; i++) {
            M2Mover mv = mp.AMov[i];
            if (mv is NelEnemy enemy) {
                var anm = enemy.AnmP;
                if (anm is not null) {
                    EnemyInfo info = generateEnemyInfo(enemy);
                    list.Add(info);
                }
            }
        }
        Dispatcher.send(new ClientBoundEnemySyncPacket() {
            enemyInfos = list.ToArray()
        });
    }

    [CanBeNull]
    public static EnemyInfo generateEnemyInfo(NelEnemy en) {
        var anm = en.AnmP;
        PxlFrame frame = anm?.getCurrentDrawnFrame();
        if (frame is null || anm.Anm is null) {
            return null;
        }
        PxlSequence sequence = frame?.pSq;
        PxlPose pose = frame?.pPose;
        PxlCharacter character = frame?.pChar;
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
        EnemyBoundingBox bbox = new() {
            width = en.sizex, height = en.sizey, // well actually not needed?
            x = x, y = y
        };
        return new() {
            name = en.name, key = en.key,
            ax = anm.Anm.mv_anmx, ay = anm.Anm.mv_anmy,
            x = en.x, y = en.y,
            vx = en.vx, vy = en.vy,
            frameIndex = frame.index,
            sequenceAim = sequence.aim,
            poseTitle = pose.title,
            characterTitle = character.title,
            hp = en.hp, hpmax = en.maxhp,
            mp = en.mp, mpmax = en.maxmp,
            scaleX = anm.Anm.scaleX, scaleY = anm.Anm.scaleY,
            rotationR = anm.rotationR,
            aimInt = (int)en.aim,
            bbox = bbox,
        };
    }
    [HarmonyPatch(typeof(M2MovRenderContainer), "RenderWholeMover")]
    [HarmonyPostfix]
    public static void renderWholeMover(ref ProjectionContainer JCon, ref Camera Cam, ref int draw_id,
        ref List<M2RenderTicket>[] ___AADob) {
        if (Cam.name != "M2D Camera -mover") return;
        if (enemyInfos == null) return;
        if (!Frontend.IsConnected) return;
        GL.LoadProjectionMatrix(JCon.CameraProjectionTransformed);
        foreach (EnemyInfo op in enemyInfos) {
            PxlCharacter chara = PxlsLoader.getPxlCharacter(op.characterTitle);
            if ((chara?.isLoadCompleted() ?? false) == false) chara = null;
            PxlPose pose = chara?.getPoseByName(op.poseTitle);
            PxlSequence seq = pose?.getSequence(op.sequenceAim);
            PxlFrame frame = seq?.getFrame(op.frameIndex);
            float screenX = op.ax, screenY = op.ay;
            toScreen(ref screenX, ref screenY);
            op.x += op.vx;
            op.y += op.vy;
            op.ax += op.vx;
            op.ay += op.vy;
            var mtr = MTRX.getMI(frame)?.getMtr();
            if (frame is null || mtr is null) {
                // pure shit why?????????????????????????????????????????????????????????????????
                NDAT.getResources(SceneGame.M2D, op.key);
                MeshDrawer md = new();
                md.activate("mdmv", UnifontRenderer.glyphMaterial, false, C32.d2c(0xCC3F3F00));
                UnifontRenderer.drawCenter(md, "这里应该有个魔族的。", screenX, screenY, 1, 1, true);
                UnifontRenderer.drawCenter(md, "太神奇了 居然没渲染出来", screenX, screenY + 16, 1, 1, true);
                UnifontRenderer.drawCenter(md, $"{op.key} {op.name}", screenX, screenY + 32, 1, 1, true);
                BLIT.RenderToGLImmediate001(md, setpass: true);
            } else {
                MeshDrawer md = new();
                md.activate("mdmv", mtr, false, C32.d2c(0xCC3F3F00));
                md.RotaPF(screenX, screenY, op.scaleX, op.scaleY, op.rotationR, frame);
                BLIT.RenderToGLImmediate001(md, setpass: true);
            }
        }
    }
}