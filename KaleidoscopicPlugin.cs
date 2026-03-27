using System;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;

namespace Kaleidoscopic;

[BepInPlugin("org.aliceincradle.kaleidoscopic", "Kaleidoscopic", "0.1.0")]
public class KaleidoscopicPlugin : BaseUnityPlugin {
    internal static KaleidoscopicPlugin INSTANCE = null;
    internal static ManualLogSource LOGGER = null;
    internal void Awake() {
        this.Logger.LogInfo("loading Kaleidoscopic plugin");
        try {
            INSTANCE = this;
            LOGGER = this.Logger;
            info("KLogger info finished");
            
        } catch (Exception ex) {
            this.Logger.LogError(ex.ToString());
            Application.Quit("Kaleidoscopic".GetHashCode());
            return;
        }
        this.Logger.LogInfo("Kaleidoscopic plugin load complete");
    }
    internal void Update() {
        
    }
}