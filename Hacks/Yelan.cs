using HarmonyLib;
using nel;

namespace Kaleidoscopic.Hacks;

[Module("取胜者，大小通吃 / 爆炸箭")]
public static class Yelan {
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MgWhiteArrow), "run")]
    public static void onRun3(MgWhiteArrow __instance, MagicItem Mg) {
        if (Mg.phase >= 0) return;
        MagicItem m = Mg.MGC.setMagic(Mg.Caster, MGKIND.FIREBALL, MGHIT.PR | MGHIT.IMMEDIATE);
        m.sx = Mg.sx;
        m.sy = Mg.sy;
        if ((Mg.Mp.M2D as NelM2DBase)?.getPrNoel() is PR pr)
        {
            pr.Skill.prepareMagicForCooking(m, m, false);
            int add0 = 0;
            pr.Skill.getOverChargeSlots().getMana(224f, ref add0);
        }
        m.run(0f);
        m.t = 140f;
        // m.Atk1.hpdmg0 = (int)(m.Atk1.hpdmg0 * 1.56);
        // m.Mn._1.thick *= 1.56f;
        m.run(0f);
        Mg.kill();
    }
}