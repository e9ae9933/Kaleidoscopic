using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

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
        } catch (Exception ex) {
            this.Logger.LogError(ex.ToString());
            Application.Quit("Kaleidoscopic".GetHashCode());
            return;
        }
        info("Kaleidoscopic plugin load complete");
    }
    internal void Update() {
        // debug("Kaleidoscopic plugin update");
    }
}