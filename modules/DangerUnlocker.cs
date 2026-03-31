using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using nel;

namespace Kaleidoscopic.modules;

[Module("解锁危险度 160 限制")]
public class DangerUnlocker {
    [HarmonyPatch(typeof(NightController), "getDangerMeterVal")]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
        foreach (CodeInstruction inst in instructions) {
            if (inst.LoadsConstant(160)) {
                inst.opcode = OpCodes.Ldc_I4;
                inst.operand = 16000;
            }
            yield return inst;
        }
    }
}