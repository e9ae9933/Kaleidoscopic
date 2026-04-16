using System;
using HarmonyLib;
using m2d;
using nel;
using XX;

namespace Kaleidoscopic.Hacks;

[Module("忿怒的报偿 / TP-Aura")]
public static class Neuvilette {
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PR), "runPre")]
    public static void onRunPre(PR __instance) {
        PR pr = __instance;
        if (true) {
            M2PrSkill skill = __instance.Skill;
            MagicItem mg = skill.getCurMagic();
            if (mg == null || !(skill.getChantCompletedRatio() >= 1) || mg.kind != MGKIND.WHITEARROW) return;
            if (IN.totalframe % 5 != 0) return;
            Map2d mp = __instance.Mp;
            float d = float.PositiveInfinity;
            M2Mover target = null;
            foreach (M2Mover mv in mp.getVectorMover())
                if (mv is NelEnemy && !mv.destructed) {
                    float d2 = distancef(mv.x, mv.y, pr.x, pr.y);
                    if (d > d2) {
                        target = mv;
                        d = d2;
                    }
                }
            float sa = randAngle();
            float tx = target?.x ?? pr.x + cosf(sa), ty = target?.y ?? pr.y + sinf(sa);
            float q = sinf(3 * sa) * 0.1f + 1;
            float r = 5.25f * q, r2x = 4 * q, r2y = 2 * q;
            debug($"target is {target}");
            pr.Skill.PtcVar("cx", __instance.x)
                .PtcVar("cy", __instance.y)
                .PtcVar("time", 36f / 2 / 1.5f);
            pr.Skill.PtcSTTimeFixed("burst_prepare", 0f, PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.FOLLOW_C);
            float x = -r2x * cosf(sa), y = r2y * sinf(sa);
            if (target != null) {
                pr.moveBy(tx - pr.x - r * cosf(sa), ty - pr.y + r * sinf(sa));
            }
            __instance.NM2D.Cam.setQuake(40, 20, 0);
            for (int p = 0; p < 1; p++) {
                MagicItem mg3 = mg.createNewMagic(null, MGKIND.WHITEARROW, x, y, false);
                mg3.reduce_mp = 20;
                mg3.run(1);
                for (int i = 0; i < 100 && mg3.phase < 2; i += 10)
                    mg3.run(10);
                mg3.sa = p == 0 ? atan2f(y - 2.5f, -x) : randAngle();
                mg3.Atk0.hpdmg0 = mg3.Atk0.hpdmg0;
                // mg3.sz *= 3;
            }
        }
    }
}