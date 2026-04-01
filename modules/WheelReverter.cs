using HarmonyLib;
using m2d;
using nel;
using XX;

namespace Kaleidoscopic.modules;

[Module("回调旋风斩")]
public static class WheelReverter {
    [HarmonyPatch(typeof(M2PrSkill), nameof(M2PrSkill.publishShotgunHit))]
    [HarmonyPrefix]
    public static void publishShotgunHit(
        M2PrSkill __instance,
        MagicItem Mg,
        M2Ray.M2RayHittedItem HitItem,
        float right_agR,
        bool replace_normal,
        ref int reduce_mag,
        bool not_apply_to_hold_value) {
        // revert
        if (Mg.kind != MGKIND.PR_WHEEL) return;
        PrCaneEquip EqCane = __instance.EqCane;
        MKind mkind = MKind.Get(__instance.CurMg.kind);
        // int v1 = X.IntR(X.NI3_02L(14f, 10f, 6f, EqCane.stability));
        int v1 = 7;
        // int red =
        //     X.Mx(v1,
        //         X.IntC(X.NIL(1f, 0.4f, EqCane.stability, 2f) *
        //                X.Mx((float)((double)X.Mn(EqCane.mp_use_ratio, 1f) * (double)mkind.reduce_mp * 0.20000000298023224),
        //                    __instance.mp_hold * 0.5f)));
        
        //	publishShotgunHit(Mg, HitItem, MathF.PI * -3f / 25f, replace_normal: true,
        //X.Mx(7, X.IntC(X.NI(1f, 0.2f, EqCane.stability) * X.Mx(X.Mn(EqCane.mp_use_ratio, 1f)
        //* (float)kindData.reduce_mp * 0.2f, mp_hold * 0.5f))));
        int red = X.Mx(v1,
            X.IntC(X.NI(1f, 0.2f, EqCane.stability) *
                   X.Mx(X.Mn(EqCane.mp_use_ratio, 1f) * mkind.reduce_mp * 0.2f, __instance.mp_hold * 0.5f)));
        reduce_mag = red;
    }
}