using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using HarmonyLib;
using PixelLiner;
using UnityEngine;
using XX;
using Random = System.Random;

namespace Kaleidoscopic.Syncs;

public class UnifontRenderer {
    private static readonly Dictionary<int, Glyph> glyphs = new();

    public static PxlCharacter glyphChara;

    public static Material glyphMaterial => MTRX.getMI(glyphChara).getMtr();

    public static void init() {
        glyphChara = new PxlCharacter("unifont" + new Random().Next() + new Random().NextDouble());
        glyphChara.loadASync(Resources.unifont65535_pxls);
        string str = UTF8Encoding.UTF8.GetString(Resources.unifont_16_0_04);
        foreach (string line in str.Split('\n')) {
            string t = line.Trim();
            if (t.Length == 0) continue;
            Glyph glyph = new(t);
            glyphs[glyph.codePoint] = glyph;
        }
    }

    [HarmonyPatch(typeof(IN), "Awake")]
    [HarmonyPostfix]
    private static void onInAwake() {
        init();
    }

    public static void draw(MeshDrawer md, string s, float x, float y, float scaleX, float scaleY, bool flipY = false) {
        if (!glyphChara.isLoadCompleted()) draw0(md, s, x, y, scaleX, scaleY, flipY);
        else draw1(md, s, x, y, scaleX, scaleY, flipY);
    }

    public static float getLengthX(string s, float scale = 1) {
        float sz = -1;
        foreach (char c in s) {
            int target = c;
            PxlFrame pxlFrame;
            if (target >= 0 && target < glyphChara.getPose(0).getSequence(0).countFrames())
                pxlFrame = glyphChara.getPose(0).getSequence(0).getFrame(target);
            else pxlFrame = glyphChara.getPose(0).getSequence(0).getFrame(0);
            sz += (pxlFrame.name == "wide" ? 16 + 1 : 8 + 1);
        }
        return Mathf.Max(0, sz * scale);
    }
    public static void drawCenter(MeshDrawer md, string s, float x, float y, float scaleX, float scaleY, bool flipY = false) {
        float sz = getLengthX(s, scaleX);
        x -= sz / 2;
        draw1(md, s, x, y, scaleX, scaleY, flipY);
    }
    public static void draw1(MeshDrawer md, string s, float x, float y, float scaleX, float scaleY, bool flipY) {
        foreach (char c in s) {
            int target = c;
            PxlFrame pxlFrame;
            if (target >= 0 && target < glyphChara.getPose(0).getSequence(0).countFrames())
                pxlFrame = glyphChara.getPose(0).getSequence(0).getFrame(target);
            else pxlFrame = glyphChara.getPose(0).getSequence(0).getFrame(0);
            float w = (pxlFrame.name == "wide" ? 16 : 8) * scaleX;
            md.RotaPF(x + w/2, y, scaleX, scaleY, 0, pxlFrame);
            x += (pxlFrame.name == "wide" ? 16 + 1 : 8 + 1) * scaleX;
        }
    }

    public static void draw0(MeshDrawer md, string s, float x, float y, float scaleX, float scaleY, bool flipY) {
        try {
            foreach (char c in s) {
                int target = c;
                if (!glyphs.ContainsKey(c)) target = 0;
                Glyph glyph = glyphs[target];
                if (!glyphs.TryGetValue(target, out glyph)) {
                    Console.WriteLine("failed on codepoint " + target);
                    continue;
                }
                for (int i = 0; i < 16; i++)
                for (int j = 0; j < glyph.width; j++)
                    if (glyph.data[i, j])
                        md.RectBL(x + j * scaleX, y + (flipY ? 15 - i : i) * scaleY, scaleX, scaleY);
                x += (glyph.width + 1) * scaleX;
            }
        } catch (Exception e) {
            Console.WriteLine(e);
        }
    }

    private class Glyph {
        public readonly int codePoint;
        public readonly bool[,] data;
        public readonly int width;

        public Glyph(string s) {
            string[] str = s.Split(':');
            this.codePoint = int.Parse(str[0], NumberStyles.HexNumber);
            if (str[1].Length == 32) this.width = 8;
            else if (str[1].Length == 64) this.width = 16;
            else throw new FormatException("having " + s);
            this.data = new bool[16, this.width];
            int top = 0;
            for (int i = 0; i < 16; i++)
            for (int j = 0; j < this.width; j += 4) {
                char c = str[1][top++];
                int val = Convert.ToByte(c + "", 16);
                this.data[i, j] = (val & 8) != 0;
                this.data[i, j + 1] = (val & 4) != 0;
                this.data[i, j + 2] = (val & 2) != 0;
                this.data[i, j + 3] = (val & 1) != 0;
            }
        }
    }
}