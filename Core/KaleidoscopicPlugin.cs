using System;
using System.Windows.Forms;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Kaleidoscopic.Core;
using Kaleidoscopic.Syncs;
using m2d;
using nel;
using UnityEngine;
using Application = UnityEngine.Application;

namespace Kaleidoscopic;

[BepInPlugin("org.aliceincradle.kaleidoscopic", "Kaleidoscopic", "0.2.0")]
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
            GeneralConfigs.bind(Config);
            ModuleManager.init(this.Config);
            if (this.Config.Bind($"网络", $"启用网络", true).Value) {
                Frontend.Start();
            }
            HARMONY.PatchAll(typeof(GeneralPatches));
            HARMONY.PatchAll(typeof(UnifontRenderer));
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
        Frontend.Tick();
        Dispatcher.handleAll();
    }
}