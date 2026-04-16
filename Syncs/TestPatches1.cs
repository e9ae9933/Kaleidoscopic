using System.Collections.Generic;
using HarmonyLib;
using m2d;
using nel;
using PixelLiner;
using UnityEngine;
using XX;

namespace Kaleidoscopic.Syncs;

// [Module("testpatches")]
public class TestPatches1 {
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PR), nameof(PR.runUi))]
    public static void runUi(PR __instance) {
        PR pr = __instance;
        PxlFrame frame = pr?.getAnimator()?.Anm?.getCurrentDrawnFrame();
        if (frame is not null) {
            PxlSequence sequence = frame.pSq;
            PxlPose pose = frame.pPose;
            PxlCharacter character = frame.pChar;
            info("frame", IN.totalframe, frame.ToString(), frame.name, frame.index);
            info("sequence", sequence.ToString(), sequence.aim);
            info("pose", pose.ToString(), pose.title);
            info("character", character.ToString(), character.title);
            frameIndex = frame.index;
            sequenceAim = sequence.aim;
            poseTitle = pose.title;
            characterTitle = character.title;
        }
    }

    private static int frameIndex;
    private static int sequenceAim;
    private static string poseTitle;
    private static string characterTitle;

    [HarmonyPatch(typeof(M2MovRenderContainer), "RenderWholeMover")]
    [HarmonyPostfix]
    public static void renderWholeMover(ref ProjectionContainer JCon, ref Camera Cam, ref int draw_id,
        ref List<M2RenderTicket>[] ___AADob) {
        if (Cam.name != "M2D Camera -mover") return;
        PxlCharacter chara = PxlsLoader.getPxlCharacter(characterTitle);
        PxlPose pose = chara?.getPoseByName(poseTitle);
        PxlSequence seq = pose?.getSequence(sequenceAim);
        PxlFrame frame = seq?.getFrame(frameIndex);
        info(chara, pose, seq, frame);
        if (frame is null) return;
        Material mat = MTRX.getMI(frame).getMtr();
        MeshDrawer md = new();
        md.activate("test", mat, false, C32.d2c(0xFFFFFFFFU));
        md.RotaPF(0, 0, 1, 1, 0, frame);
        GL.LoadProjectionMatrix(JCon.CameraProjectionTransformed);
        BLIT.RenderToGLImmediate001(md, setpass: true);
    }
}