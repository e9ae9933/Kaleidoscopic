using System;
using System.Linq;
using HarmonyLib;
using nel;

namespace Kaleidoscopic.Modules;

[Module("转轮自动转到最优选择")]
public static class ReelSetter {
    [HarmonyPatch(typeof(ReelExecuter), nameof(ReelExecuter.applyEffectToIK))]
    [HarmonyPrefix]
    public static void ApplyEffectToIK(ReelExecuter __instance, ReelExecuter Reel) {
        try {
            // if we can add item...
            if (__instance.IKRow == null) return;
            int count0 = __instance.IKRow.count;
            int grade0 = __instance.IKRow.grade;
            // no, dp is needed.
            int id = 0;
            if (dp(__instance.Ui, count0, grade0, __instance.Ui.rotate_decide_id, ref id)) {
                Reel.content_id_dec = id;
                string target = Reel.Acontent[Reel.content_id_dec % Reel.Acontent.Length];
            }
        } catch (Exception e) {
            error(e);
        }
    }
    private static bool dp(UiReelManager ui, int c0, int g0, int from, ref int id) {
        if (ui == null) return false;
        if (from < 0) return false;
        ReelExecuter[] r0 = ui.AReel?.ToArray();
        if (r0 == null || r0.Length <= 0 || r0.Length <= from) return false;
        // from .
        ReelExecuter[] reels = r0.Skip(from).ToArray();
        int n = reels.Length;
        int[,] f = new int[n + 1, 5];
        int[,] pre = new int[n + 1, 5];
        int[,] chs = new int[n + 1, 5];
        for (int i = 0; i <= n; i++)
        for (int j = 0; j <= 4; j++)
            f[i, j] = -100000;
        f[0, g0] = c0;
        for (int i = 0; i < n; i++) {
            for (int j = 0; j <= 4; j++) {
                if (f[i, j] < 0) continue;
                if (reels[i]?.Acontent == null) return false;
                int m = reels[i].Acontent.Length;
                int[] tk = new int[m];
                for (int t = 0; t < m; t++) tk[t] = t;
                Array.Sort(tk, (u, v) =>
                    string.Compare(reels[i].Acontent[u], reels[i].Acontent[v], StringComparison.Ordinal)
                );
                foreach (int k in tk) {
                    int c = f[i, j], g = j;
                    apply(reels[i].Acontent[k], ref c, ref g);
                    if (f[i + 1, g] <= c) {
                        f[i + 1, g] = c;
                        chs[i + 1, g] = k;
                        pre[i + 1, g] = j;
                    }
                }
            }
        }
        int lg = -1;
        for (int j = 4; j >= 0; j--)
            if (f[n, j] > 0) {
                lg = j;
                info("final target", f[n, j]);
                break;
            }
        if (lg == -1) return false; // ???
        for (int i = n; i >= 2; i--) {
            lg = pre[i, lg];
        }
        id = chs[1, lg];
        return true;
    }
    private static void apply(string str, ref int c, ref int g) {
        ReelExecuter.EFFECT type;
        if (!Enum.TryParse(str, false, out type)) return;
        switch (type) {
            case ReelExecuter.EFFECT.GRADE1:
            case ReelExecuter.EFFECT.GRADE2:
            case ReelExecuter.EFFECT.GRADE3:
            case ReelExecuter.EFFECT.GRADE4:
                g = Math.Min(4, g + type - ReelExecuter.EFFECT.GRADE0);
                break;
            case ReelExecuter.EFFECT.COUNT_ADD1:
            case ReelExecuter.EFFECT.COUNT_ADD2:
            case ReelExecuter.EFFECT.COUNT_ADD3:
            case ReelExecuter.EFFECT.COUNT_ADD4:
            case ReelExecuter.EFFECT.COUNT_ADD5:
                c = c + type - ReelExecuter.EFFECT.COUNT_ADD1 + 1;
                break;
            case ReelExecuter.EFFECT.COUNT_MUL2:
                c *= 2;
                break;
        }
    }
}