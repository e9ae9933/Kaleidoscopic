using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using BepInEx.Configuration;
using HarmonyLib;

namespace Kaleidoscopic;

public static class ModuleManager {
    private static void createForNamespace(ConfigFile config, string ns, string nsdesc) {
        IEnumerable<Type> moduleTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.Namespace == ns && t.IsClass);

        foreach (Type type in moduleTypes) {
            ModuleAttribute attribute = type.GetCustomAttribute<ModuleAttribute>();
            if (attribute is null) continue;
            string desc = attribute.description;
            ConfigEntry<bool> switchConfig = config.Bind(
                $"{nsdesc}",
                $"{desc}",
                true,
                $"{type.FullName}"
            );

            Harmony moduleHarmony = new($"org.aliceincradle.kaleidoscopic.module.{type.Name.ToLower()}");
            Action enable = () => {
                try {
                    new PatchClassProcessor(moduleHarmony, type, true).Patch();
                    info($"[>] enabled module: {type.Name}");
                } catch (Exception e) {
                    warning("enable failed", type.FullName, e);
                    MessageBox.Show($"无法启动补丁 {desc}\n{e}", "Kaleidoscopic", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };
            Action disable = () => {
                moduleHarmony.UnpatchSelf();
                info($"[<] disabled module: {type.Name}");
            };
            if (switchConfig.Value) {
                enable();
            }
            switchConfig.SettingChanged += (sender, args) => {
                if (switchConfig.Value) {
                    enable();
                } else {
                    disable();
                }
            };
        }}
    
    public static void init(ConfigFile config) {
        createForNamespace(config,"Kaleidoscopic.Modules","实用功能");
    }
}