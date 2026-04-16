global using static Kaleidoscopic.StrictMath;
using System;
using m2d;
using nel;

namespace Kaleidoscopic;

public class StrictMath {
    public static readonly Random random = new(998244353);

    public static int randInt(int leftInclusive, int rightExclusive) {
        return random.Next(leftInclusive, rightExclusive);
    }

    public static float randFloat() {
        // why there's no 0x1.0p-24f in c#
        return randInt(0, 1 << 24) * 5.9604645e-8F;
    }

    public static float randAngle() {
        return (float)(random.NextDouble() * 2 * Math.PI);
    }

    public static float randGaussian() {
        return (float)randGaussian0();
    }

    public static float sinf(double x) {
        return (float)Math.Sin(x);
    }
    public static float cosf(double x) {
        return (float)Math.Cos(x);
    }
    public static float tanf(double x) {
        return (float)Math.Tan(x);
    }
    public static float sqrtf(double x) {
        return (float)Math.Sqrt(x);
    }

    public static float atan2f(double y, double x) {
        return (float)Math.Atan2(y, x);
    }
    public static float distancef(double x1, double y1, double x2, double y2) {
        return sqrtf((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
    }

    public static void toScreen(ref float x, ref float y) {
        NelM2DBase m2d = M2DBase.Instance as NelM2DBase;
        Map2d mp = m2d.curMap;
        x = mp.ux2effectScreenx(mp.map2ux(x)) * 64;
        y = mp.uy2effectScreeny(mp.map2uy(y)) * 64;
    }

    private static double randGaussian0() {
        double u1 = 0;
        while (u1 <= 0) u1 = random.NextDouble();
        double u2 = random.NextDouble();
        return Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2);
    }
}