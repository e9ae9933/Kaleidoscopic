using System;
using System.Windows.Forms;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Application = UnityEngine.Application;

namespace Kaleidoscopic;

[BepInPlugin("org.aliceincradle.kaleidoscopic", "Kaleidoscopic", "0.1.0")]
public class KaleidoscopicPlugin : BaseUnityPlugin {
    internal static KaleidoscopicPlugin INSTANCE;
    internal static ManualLogSource LOGGER;
    internal static Harmony HARMONY;
    internal void Awake() {
        this.Logger.LogInfo("loading Kaleidoscopic plugin");
        try {
            INSTANCE = this;
            LOGGER = this.Logger;
            HARMONY = new Harmony("org.aliceincradle.kaleidoscopic");
            ModuleManager.init(this.Config);
            HARMONY.PatchAll(typeof(GeneralPatches));
        } catch (Exception ex) {
            this.Logger.LogError(ex.ToString());
            string text = $"无法加载插件 万华镜/Kaleidoscopic\n{ex}";
            MessageBox.Show(text, "Kaleidoscopic catastrophe", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            Application.Quit("Kaleidoscopic".GetHashCode());
            return;
        }
        info("Kaleidoscopic plugin load complete");
    }
    internal void Update() {
        // debug("Kaleidoscopic plugin update");
    }
}