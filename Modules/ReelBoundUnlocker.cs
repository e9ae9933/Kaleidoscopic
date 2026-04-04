using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using nel;

namespace Kaleidoscopic.Modules;

[Module("解锁转轮物品获取 99 上限")]
public static class ReelBoundUnlocker {
    [HarmonyPatch(typeof(ReelExecuter), nameof(ReelExecuter.applyEffectToIK))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
        foreach (CodeInstruction inst in instructions) {
            if (inst.LoadsConstant(99)) {
                inst.opcode = OpCodes.Ldc_I4;
                inst.operand = 100000;
            }
            yield return inst;
        }
    }
}