using System.Collections.Generic;
using HarmonyLib;
using Kaleidoscopic.Core;
using Kaleidoscopic.Syncs.Packets;
using m2d;
using nel;
using PixelLiner;
using UnityEngine;
using XX;

namespace Kaleidoscopic.Syncs;

[Module("联机核心模块")]
public static class MultiHandler {
    public static volatile PlayerInfo[] otherPlayers = [];
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PR), nameof(PR.runUi))]
    public static void runUi(PR __instance) {
        PR pr = __instance;
        PxlFrame frame = pr?.getAnimator()?.Anm?.getCurrentDrawnFrame();
        if (frame is not null) {
            PxlSequence sequence = frame.pSq;
            PxlPose pose = frame.pPose;
            PxlCharacter character = frame.pChar;
            string caneKey = pr?.Skill?.getCurrentCaneEquip().cane_key;
            Dispatcher.send(new ClientBoundVlanChangedPacket() {
                vlan = pr.Mp.key.GetHashCode()
            });
            PlayerInfo info = createPlayerInfo(pr);
            if (info != null)
                Dispatcher.send(new ClientBoundPlayerSyncPacket() {
                    info = info
                });
        }
    }

    public static PlayerInfo createPlayerInfo(PR pr) {
        if (pr == null) return null;
        PxlFrame frame = pr?.getAnimator()?.Anm?.getCurrentDrawnFrame();
        if (frame is null) return null;
        PxlSequence sequence = frame.pSq;
        PxlPose pose = frame.pPose;
        PxlCharacter character = frame.pChar;
        string caneKey = pr?.Skill?.getCurrentCaneEquip().cane_key;
        return new() {
            playerName = GeneralConfigs.multiName.Value,
            ax = pr.Anm.mv_anmx, ay = pr.Anm.mv_anmy,
            caneName = caneKey,
            x = pr.x, y = pr.y,
            vx = pr.vx, vy = pr.vy,
            frameIndex = frame.index,
            sequenceAim = sequence.aim,
            poseTitle = pose.title,
            characterTitle = character.title,
        };
    }

    [HarmonyPatch(typeof(M2MovRenderContainer), "RenderWholeMover")]
    [HarmonyPostfix]
    public static void renderWholeMover(ref ProjectionContainer JCon, ref Camera Cam, ref int draw_id,
        ref List<M2RenderTicket>[] ___AADob) {
        if (Cam.name != "M2D Camera -mover") return;
        if (otherPlayers == null) return;
        if (Frontend.IsConnected) {
        } else return;
        foreach (PlayerInfo op in otherPlayers) {
            render1(JCon, op);
        }
        foreach (PlayerInfo op in otherPlayers) {
            render2(JCon, op);
        }
        if (createPlayerInfo(SceneGame.M2D.PlayerNoel) is { } cpi) {
            render2(JCon, cpi, true);
        }
        foreach (PlayerInfo op in otherPlayers) {
            op.x += op.vx;
            op.y += op.vy;
            op.ax += op.vx;
            op.ay += op.vy;
        }
    }
    public static void render2(ProjectionContainer JCon, PlayerInfo op, bool self = false) {
        MeshDrawer mdGraphics = new(), mdText = new();
        mdText.activate("name_text", UnifontRenderer.glyphMaterial, false, C32.d2c(self?0xFF3FFF7F:0xFFFFFFFFU));
        mdGraphics.activate("name_graphics", MTRX.MtrMeshNormal, false, C32.d2c(0xCF111111U));

        float screenX = op.x, screenY = op.y - 2;
        toScreen(ref screenX, ref screenY);

        mdGraphics.Box(screenX, screenY, op.playerName.Length * 9 + 21, 30);
        UnifontRenderer.drawCenter(mdText, op.playerName, screenX, screenY, 1f, 1f, true);

        GL.LoadProjectionMatrix(JCon.CameraProjectionTransformed);
        BLIT.RenderToGLImmediate001(mdGraphics, setpass: true);
        BLIT.RenderToGLImmediate001(mdText, setpass: true);
    }
    public static void render1(ProjectionContainer JCon, PlayerInfo op) {
        PxlCharacter chara = PxlsLoader.getPxlCharacter(op.characterTitle);
        PxlPose pose = chara?.getPoseByName(op.poseTitle);
        PxlSequence seq = pose?.getSequence(op.sequenceAim);
        PxlFrame frame = seq?.getFrame(op.frameIndex);

        if (frame is null) return;

        Material baseMat = MTRX.getMI(frame).getMtr();

        // 【修改1：图层拆分】把本体画笔拆成“后层”和“前层”
        MeshDrawer mdBaseBehind = new();
        mdBaseBehind.activate("ghost_base_b", baseMat, false, C32.d2c(0xFFFFFFFFU));

        MeshDrawer mdBaseFront = new();
        mdBaseFront.activate("ghost_base_f", baseMat, false, C32.d2c(0xFFFFFFFFU));

        MeshDrawer mdCane = new();
        bool hasCane = false;

        // 3. 逐图层拆解并绘制
        int layerCount = frame.countLayers();
        for (int i = 0; i < layerCount; i++) {
            PxlLayer layer = frame.getLayer(i);

            // info("layer name", layer.name, layer.getImportSource().name);

            // 屏蔽特效层
            if (layer.name.StartsWith("rodeff")) continue;

            bool isrod = layer.getImportSource().name.StartsWith("rod");
            float screenX = op.ax, screenY = op.ay;
            toScreen(ref screenX, ref screenY);

            CaneManager.CaneItem currentCane = CaneManager.Get(op.caneName) ?? CaneManager.DefaultCane;
            // 拦截：如果是法杖图层，且我们确实有装备法杖
            if (isrod && currentCane != null) {
                // info("found rod so setting");
                CaneManager.ANGLE angle = CaneManager.ANGLE._MAX;
                PxlLayer caneBaseLayer = CaneManager.DefaultCane.switchImage(layer, ref angle);
                PxlLayer actualCaneLayer = caneBaseLayer;

                if (currentCane != CaneManager.DefaultCane && angle != CaneManager.ANGLE._MAX) {
                    actualCaneLayer = currentCane.switchImage(caneBaseLayer, ref angle);
                }

                if (actualCaneLayer != null && actualCaneLayer.Img != null) {
                    // 【修改2：偏移计算】动态计算法杖的握持偏移，解决错位/脱手问题
                    float shiftX = 0f;
                    float shiftY = 0f;

                    if (currentCane != CaneManager.DefaultCane) {
                        float grabX = currentCane.grab_shift_x * currentCane.grab_shift_level(angle) * layer.zmx;
                        float grabY = currentCane.grab_shift_y;

                        // 根据当前图层(手臂)的旋转角度，计算出贴合手心的实际二维偏移
                        if (grabX != 0f || grabY != 0f) {
                            Vector2 rotatedShift = X.ROTV2e(new Vector2(grabX, grabY), -layer.rotR);
                            shiftX = rotatedShift.x;
                            shiftY = rotatedShift.y;
                        }
                    }

                    // 获取法杖独立的材质
                    Material caneMat = MTRX.getMI(actualCaneLayer.Img).getMtr();
                    mdCane.activate("ghost_cane", caneMat, false, C32.d2c(0xFFFFFFFFU));

                    // 传入算好的 shiftX 和 shiftY
                    mdCane.RotaL(screenX + shiftX, screenY + shiftY, layer, false, false, 0, actualCaneLayer.Img, false);
                    hasCane = true;
                    continue;
                }
            }

            // 【修改3：图层分流】根据绘制顺序把图层分流到不同的画笔
            if (!hasCane) {
                // 如果还没画法杖，说明是后脑勺、后手臂、身体
                mdBaseBehind.RotaL(screenX, screenY, layer);
            } else {
                // 如果法杖已经画了，说明当前是盖在法杖上面的前手指、前袖子
                mdBaseFront.RotaL(screenX, screenY, layer);
            }
        }

        // 4. 按严格的 Z-Order 顺序提交 GL 渲染
        GL.LoadProjectionMatrix(JCon.CameraProjectionTransformed);

        // 第一层：画背后的身体
        BLIT.RenderToGLImmediate001(mdBaseBehind, setpass: true);

        // 第二层：画中间的法杖
        if (hasCane) {
            BLIT.RenderToGLImmediate001(mdCane, setpass: true);
        }

        // 第三层：画盖在法杖前面的手指，达成完美握持效果
        BLIT.RenderToGLImmediate001(mdBaseFront, setpass: true);
    }
}