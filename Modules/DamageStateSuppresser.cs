using HarmonyLib;
using nel;

namespace Kaleidoscopic.Modules;

[Module("受到轻微攻击时不打断动作")]
public static class DamageStateSuppresser {
    // [HarmonyPrefix]
    // [HarmonyPatch(typeof(M2PrADmg), nameof(M2PrADmg.applyDamage),
    //     [typeof(NelAttackInfo), typeof(HITTYPE), typeof(bool),
    //     typeof(string), typeof(bool), typeof(bool)],
    //     [ArgumentType.Normal,ArgumentType.Ref,ArgumentType.Normal,
    //     ArgumentType.Normal,ArgumentType.Normal,ArgumentType.Normal])]
    // public static void applyDamage(
    //     M2PrADmg __instance,
    //     NelAttackInfo Atk,
    //     ref HITTYPE add_hittype,
    //     bool force,
    //     string fade_key,
    //     bool decline_ui_additional_effect,
    //     bool from_press_damage) {
    //     Atk.burst_vx *=10;
    //     Atk.burst_vy = 0;
    //     info("atk",Atk,Atk.knockback_len,add_hittype);
    // }
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PR), "changeState", typeof(PR.STATE), typeof(PR.STATE))]
    public static void PR_changeState(PR __instance, ref bool __runOriginal, ref PR.STATE _state, PR.STATE prestate) {
        info("state ", _state, prestate);
        if (_state == PR.STATE.DAMAGE) {
            __runOriginal = false;
        }
    }
}