using AliceInCradleHack.config;
using AliceInCradleHack.module.modules.client;
using AliceInCradleHack.module.modules.combat;
using AliceInCradleHack.module.modules.misc;
using AliceInCradleHack.module.modules.visual;
using AliceInCradleHack.utils.client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AliceInCradleHack.module
{
    /// <summary>
    /// Module manager (singleton). Handles module registration, initialization,
    /// enable/disable lifecycle and settings access.
    /// </summary>
    public class ModuleManager
    {
        private readonly ConcurrentDictionary<string, Module> _modules = new();

        private static readonly Lazy<ModuleManager> _lazyInstance = new(() => new ModuleManager());
        public static ModuleManager Instance => _lazyInstance.Value;

        private ModuleManager() { }

        /// <summary>
        /// Registers and initializes all built-in modules, then loads their configs.
        /// </summary>
        public void Initialize()
        {
            var builtInModules = new List<Module>
            {
                new ModuleMosaicRemove(),
                new ModuleDiscordRPC(),
                new ModuleKillSound(),
                new ModuleCritical(),
                new ModuleVelocity(),
                new ModuleWebUi(),
                // Add other module instances here
            };

            foreach (var module in builtInModules)
            {
                RegisterModule(module);
            }

            ConfigSystem.LoadAll();
            ApplyEnabledStates();
        }

        /// <summary>
        /// Gets all registered modules.
        /// </summary>
        public IEnumerable<Module> GetAllModules() => _modules.Values;

        /// <summary>
        /// Registers a module: creates its root config, auto-registers its setting fields
        /// and initializes it. Modules that fail to initialize are removed.
        /// </summary>
        public void RegisterModule(Module module)
        {
            if (module == null)
            {
                throw new ArgumentNullException(nameof(module), "Module instance cannot be null");
            }

            if (_modules.TryAdd(module.Name, module))
            {
                try
                {
                    module.Settings = ConfigSystem.Root(new Config(module.Name, module.Description));
                    module.AutoRegisterSettings();
                    module.EnabledValue = module.Settings.Boolean(
                        "__IsEnabled", module.IsEnabled, "Module enabled state", doNotInclude: true);
                    module.Initialize();
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to initialize module {module.Name}", ex);
                    _modules.TryRemove(module.Name, out _);
                }
            }
            else
            {
                Log.Warn($"Module already exists, skip registration {module.Name}");
            }
        }

        /// <summary>
        /// Applies the persisted enabled states after configs have been loaded.
        /// </summary>
        private void ApplyEnabledStates()
        {
            foreach (var module in _modules.Values)
            {
                bool shouldBeEnabled = module.EnabledValue?.Get() ?? module.IsEnabled;
                if (shouldBeEnabled && !module.IsEnabled)
                    EnableModule(module.Name);
                else if (!shouldBeEnabled && module.IsEnabled)
                    DisableModule(module.Name);
            }
        }

        /// <summary>
        /// Enables the specified module.
        /// </summary>
        public void EnableModule(string moduleName)
        {
            if (string.IsNullOrWhiteSpace(moduleName)) return;

            if (_modules.TryGetValue(moduleName, out var module) && !module.IsEnabled)
            {
                try
                {
                    module.Enable();
                    module.IsEnabled = true;
                    module.EnabledValue?.Set(true);
                    Notification.ShowNotificationByUILog($"Enabled {module.Name}", nel.UILogRow.TYPE.ALERT);
                    StoreModuleConfig(module);
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to enable module {moduleName}", ex);
                }
            }
        }

        /// <summary>
        /// Disables the specified module.
        /// </summary>
        public void DisableModule(string moduleName)
        {
            if (string.IsNullOrWhiteSpace(moduleName)) return;

            if (_modules.TryGetValue(moduleName, out var module) && module.IsEnabled)
            {
                try
                {
                    module.Disable();
                    module.IsEnabled = false;
                    module.EnabledValue?.Set(false);
                    Notification.ShowNotificationByUILog($"Disabled {module.Name}", nel.UILogRow.TYPE.ALERT);
                    StoreModuleConfig(module);
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to disable module {moduleName}", ex);
                }
            }
        }

        /// <summary>
        /// Toggles the enabled state of the specified module.
        /// </summary>
        public void ToggleModule(string moduleName)
        {
            if (string.IsNullOrWhiteSpace(moduleName)) return;

            if (_modules.TryGetValue(moduleName, out var module))
            {
                if (module.IsEnabled)
                {
                    DisableModule(moduleName);
                }
                else
                {
                    EnableModule(moduleName);
                }
            }
        }

        /// <summary>
        /// Gets a module by name, or null if it does not exist.
        /// </summary>
        public Module GetModuleByName(string moduleName)
        {
            if (string.IsNullOrWhiteSpace(moduleName)) return null;
            _modules.TryGetValue(moduleName, out var module);
            return module;
        }

        /// <summary>
        /// Gets all modules in the specified category (case-insensitive).
        /// </summary>
        public IEnumerable<Module> GetModulesByCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category)) return Enumerable.Empty<Module>();
            return _modules.Values.Where(m => m.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<Module> GetEnabledModules() => _modules.Values.Where(m => m.IsEnabled);

        public IEnumerable<Module> GetDisabledModules() => _modules.Values.Where(m => !m.IsEnabled);

        public bool IsModuleEnabled(string moduleName)
        {
            if (string.IsNullOrWhiteSpace(moduleName)) return false;
            return _modules.TryGetValue(moduleName, out var module) && module.IsEnabled;
        }

        /// <summary>
        /// Gets the paths of all leaf settings of a module (e.g. "Display.Window.Width").
        /// </summary>
        public string[] GetSettingPaths(string moduleName)
        {
            var module = GetModuleByName(moduleName);
            if (module?.Settings == null) return Array.Empty<string>();
            return module.Settings.GetAllLeafNodes().Select(n => n.GetPath()).ToArray();
        }

        /// <summary>
        /// Gets a module setting value by path, or null if it does not exist.
        /// </summary>
        public object GetSettingValue(string moduleName, string settingPath)
        {
            return GetModuleByName(moduleName)?.Settings?.GetValueByPath(settingPath);
        }

        /// <summary>
        /// Sets a module setting value by path and persists the module's config.
        /// </summary>
        public bool SetSettingValue(string moduleName, string settingPath, object value)
        {
            var module = GetModuleByName(moduleName);
            if (module?.Settings == null) return false;
            bool success = module.Settings.SetValueByPath(settingPath, value);
            if (success) StoreModuleConfig(module);
            return success;
        }

        /// <summary>
        /// Gets detailed information about a setting node, or null if it does not exist.
        /// </summary>
        public Value GetSettingNode(string moduleName, string settingPath)
        {
            return GetModuleByName(moduleName)?.Settings?.GetNodeByPath(settingPath);
        }

        /// <summary>
        /// Persists the module's config to disk.
        /// </summary>
        public void StoreModuleConfig(Module module)
        {
            if (module?.Settings != null)
                ConfigSystem.Store(module.Settings);
        }

        /// <summary>
        /// Exports a single module's settings to a JSON file.
        /// </summary>
        public bool ExportModuleSettings(string moduleName, string filePath)
        {
            var module = GetModuleByName(moduleName);
            if (module?.Settings == null)
            {
                Log.Warn($"Module '{moduleName}' not found");
                return false;
            }
            try
            {
                File.WriteAllText(filePath, module.Settings.ToJToken().ToString(Formatting.Indented), Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to export settings of '{moduleName}'", ex);
                return false;
            }
        }

        /// <summary>
        /// Imports a single module's settings from a JSON file.
        /// </summary>
        public bool ImportModuleSettings(string moduleName, string filePath)
        {
            var module = GetModuleByName(moduleName);
            if (module?.Settings == null)
            {
                Log.Warn($"Module '{moduleName}' not found");
                return false;
            }
            try
            {
                if (!File.Exists(filePath)) return false;
                module.Settings.FromJToken(JObject.Parse(File.ReadAllText(filePath, Encoding.UTF8)));
                StoreModuleConfig(module);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to import settings of '{moduleName}'", ex);
                return false;
            }
        }

        /// <summary>
        /// Exports all modules' settings and enabled states to a single JSON file.
        /// </summary>
        public bool ExportAllSettings(string filePath)
        {
            try
            {
                var all = new JObject();
                foreach (var module in _modules.Values)
                {
                    if (module.Settings != null)
                        all[module.Name] = module.Settings.ToJToken();
                }
                File.WriteAllText(filePath, all.ToString(Formatting.Indented), Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Failed to export all settings", ex);
                return false;
            }
        }

        /// <summary>
        /// Imports settings and enabled states from a single JSON file, toggling modules as needed.
        /// Unknown modules are skipped with a warning.
        /// </summary>
        public bool ImportAllSettings(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Log.Warn($"Settings file not found: {filePath}");
                    return false;
                }

                var all = JObject.Parse(File.ReadAllText(filePath, Encoding.UTF8));
                bool success = true;

                foreach (var property in all.Properties())
                {
                    var module = GetModuleByName(property.Name);
                    if (module?.Settings == null)
                    {
                        Log.Warn($"Module '{property.Name}' not found, skipping...");
                        success = false;
                        continue;
                    }
                    if (property.Value is not JObject moduleObj) continue;

                    module.Settings.FromJToken(moduleObj);

                    bool shouldBeEnabled = module.EnabledValue?.Get() ?? module.IsEnabled;
                    if (shouldBeEnabled && !module.IsEnabled)
                        EnableModule(module.Name);
                    else if (!shouldBeEnabled && module.IsEnabled)
                        DisableModule(module.Name);

                    StoreModuleConfig(module);
                }
                return success;
            }
            catch (Exception ex)
            {
                Log.Error("Failed to import all settings", ex);
                return false;
            }
        }

        /// <summary>
        /// Gets a module's settings as a JSON string, or null if the module does not exist.
        /// </summary>
        public string GetModuleSettingsAsJson(string moduleName)
        {
            return GetModuleByName(moduleName)?.Settings?.ToJToken().ToString(Formatting.Indented);
        }

        /// <summary>
        /// Applies settings to a module from a JSON string.
        /// </summary>
        public bool SetModuleSettingsFromJson(string moduleName, string jsonSettings)
        {
            if (string.IsNullOrWhiteSpace(jsonSettings)) return false;
            var module = GetModuleByName(moduleName);
            if (module?.Settings == null) return false;
            try
            {
                module.Settings.FromJToken(JObject.Parse(jsonSettings));
                StoreModuleConfig(module);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to apply settings of '{moduleName}'", ex);
                return false;
            }
        }
    }
}
