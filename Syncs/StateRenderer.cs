using System;
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

[Module("联机信息")]
public static class StateRenderer {
    [HarmonyPatch(typeof(M2MovRenderContainer), "RenderWholeMover")]
    [HarmonyPostfix]
    public static void renderWholeMover(ref ProjectionContainer JCon, ref Camera Cam, ref int draw_id,
        ref List<M2RenderTicket>[] ___AADob) {
        if (Cam.name != "M2D Camera -mover") return;
        if (Frontend.IsConnected) {
        } else return;
        if (!Input.GetKey(KeyCode.Tab)) return;
        var pr = SceneGame.M2D.getPrNoel();
        if(pr==null) return;
        GL.LoadProjectionMatrix(JCon.CameraProjectionTransformed);
        render2(pr);
    }
    public static void render2(PR pr) {
        MeshDrawer mdGraphics = new(), mdText = new();
        mdText.activate("rd_text", UnifontRenderer.glyphMaterial, false, C32.d2c(0xFF3FFF7F));
        mdGraphics.activate("rd_graphics", MTRX.MtrMeshNormal, false, C32.d2c(0xCF111111U));

        float screenX = pr.drawx_map, screenY = pr.drawy_map - 3f;
        toScreen(ref screenX, ref screenY);

        var v0 = KaleidoscopicPlugin.INSTANCE.Info.Metadata.Version;
        string v = $"26w17g";
        
        string text = $"{KaleidoscopicPlugin.INSTANCE.Info.Metadata.Name} {v}";
        mdGraphics.Box(screenX, screenY, UnifontRenderer.getLengthX(text) + 16, 30);
        UnifontRenderer.drawCenter(mdText, text, screenX, screenY, 1f, 1f, true);

        BLIT.RenderToGLImmediate001(mdGraphics, setpass: true);
        BLIT.RenderToGLImmediate001(mdText, setpass: true);
    }
}