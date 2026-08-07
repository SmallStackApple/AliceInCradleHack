using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AliceInCradleHack.module.settings
{
    /// <summary>
    /// Handles JSON import/export of all module settings, including module enabled states.
    /// </summary>
    public static class ModuleSettingsStore
    {
        private const string IsEnabledKey = "__IsEnabled";

        /// <summary>
        /// Exports the settings and enabled state of every module to a JSON file.
        /// </summary>
        public static bool ExportAll(IReadOnlyDictionary<string, Module> modules, string filePath)
        {
            try
            {
                var allSettings = new JObject();

                foreach (var module in modules.Values)
                {
                    var moduleSettings = new JObject
                    {
                        [IsEnabledKey] = module.IsEnabled
                    };
                    moduleSettings.Merge(module.Settings.ToJToken());
                    allSettings[module.Name] = moduleSettings;
                }

                File.WriteAllText(filePath, allSettings.ToString(Formatting.Indented), Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to export all settings: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Imports settings and enabled states from a JSON file, toggling modules as needed.
        /// Unknown modules or settings are skipped with a warning and mark the result as failed.
        /// </summary>
        public static bool ImportAll(IReadOnlyDictionary<string, Module> modules, string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"Settings file not found: {filePath}");
                    return false;
                }

                var jsonContent = File.ReadAllText(filePath, Encoding.UTF8);
                var allSettings = JObject.Parse(jsonContent);

                bool success = true;

                foreach (var moduleProperty in allSettings.Properties())
                {
                    var moduleName = moduleProperty.Name;
                    if (!modules.TryGetValue(moduleName, out var module))
                    {
                        Console.WriteLine($"Warning: Module '{moduleName}' not found, skipping...");
                        success = false;
                        continue;
                    }

                    var moduleSettingsObj = (JObject)moduleProperty.Value;

                    if (moduleSettingsObj.TryGetValue(IsEnabledKey, out var isEnabledToken))
                    {
                        bool shouldBeEnabled = isEnabledToken.ToObject<bool>();

                        if (module.IsEnabled && !shouldBeEnabled)
                        {
                            ModuleManager.Instance.DisableModule(moduleName);
                        }
                        else if (!module.IsEnabled && shouldBeEnabled)
                        {
                            ModuleManager.Instance.EnableModule(moduleName);
                        }
                    }

                    // Remove the state marker so it does not interfere with the settings import.
                    moduleSettingsObj.Remove(IsEnabledKey);

                    var moduleSettingsJson = moduleSettingsObj.ToString();
                    if (!module.Settings.FromJson(moduleSettingsJson))
                    {
                        Console.WriteLine($"Warning: Failed to import settings for module '{moduleName}'");
                        success = false;
                    }
                }

                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to import all settings: {ex.Message}");
                return false;
            }
        }
    }
}
