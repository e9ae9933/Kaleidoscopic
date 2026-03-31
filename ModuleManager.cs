using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;
using HarmonyLib;

namespace Kaleidoscopic;

public static class ModuleManager {
    public static void init(ConfigFile config) {
        IEnumerable<Type> moduleTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.Namespace == "Kaleidoscopic.modules" && t.IsClass);

        foreach (Type type in moduleTypes) {
            ModuleAttribute attribute = type.GetCustomAttribute<ModuleAttribute>();
            if (attribute is null) continue;
            string desc = attribute.description;
            ConfigEntry<bool> switchConfig = config.Bind(
                "模块",
                $"{desc}",
                false,
                $"{type.FullName}"
            );

            Harmony moduleHarmony = new($"org.aliceincradle.kaleidoscopic.module.{type.Name.ToLower()}");

            if (switchConfig.Value) {
                new PatchClassProcessor(moduleHarmony, type, true).Patch();
                info($"[+] loaded module: {type.Name}");
            }

            switchConfig.SettingChanged += (sender, args) => {
                if (switchConfig.Value) {
                    new PatchClassProcessor(moduleHarmony, type, true).Patch();
                    info($"[>] enabled module: {type.Name}");
                } else {
                    moduleHarmony.UnpatchSelf();
                    info($"[<] disabled module: {type.Name}");
                }
            };
        }
    }
}