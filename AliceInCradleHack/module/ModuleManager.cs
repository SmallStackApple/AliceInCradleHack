using AliceInCradleHack.module.modules.combat;
using AliceInCradleHack.module.modules.misc;
using AliceInCradleHack.module.modules.visuals;
using AliceInCradleHack.module.settings;
using AliceInCradleHack.utils.client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

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
        /// Registers and initializes all built-in modules.
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
                // Add other module instances here
            };

            foreach (var module in builtInModules)
            {
                RegisterModule(module);
            }
        }

        /// <summary>
        /// Gets all registered modules.
        /// </summary>
        public IEnumerable<Module> GetAllModules() => _modules.Values;

        /// <summary>
        /// Registers a module and initializes it. Modules that fail to initialize are removed.
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
                    module.Initialize();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to initialize module {module.Name}: {ex.Message}");
                    _modules.TryRemove(module.Name, out _);
                }
            }
            else
            {
                Console.WriteLine($"Module already exists, skip registration {module.Name}");
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
                    Notification.ShowNotificationByUILog($"Enabled {module.Name}", nel.UILogRow.TYPE.ALERT);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to enable module {moduleName}: {ex.Message}");
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
                    Notification.ShowNotificationByUILog($"Disabled {module.Name}", nel.UILogRow.TYPE.ALERT);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to disable module {moduleName}: {ex.Message}");
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
            return module?.Settings.GetAllLeafValues().Keys.ToArray() ?? Array.Empty<string>();
        }

        /// <summary>
        /// Gets a module setting value by path, or null if it does not exist.
        /// </summary>
        public object GetSettingValue(string moduleName, string settingPath)
        {
            return GetModuleByName(moduleName)?.Settings.GetValueByPath(settingPath);
        }

        /// <summary>
        /// Sets a module setting value by path.
        /// </summary>
        public bool SetSettingValue(string moduleName, string settingPath, object value)
        {
            var module = GetModuleByName(moduleName);
            return module != null && module.Settings.SetValueByPath(settingPath, value);
        }

        /// <summary>
        /// Gets detailed information about a setting node, or null if it does not exist.
        /// </summary>
        public SettingNode GetSettingNode(string moduleName, string settingPath)
        {
            return GetModuleByName(moduleName)?.Settings.GetNodeByPath(settingPath);
        }

        /// <summary>
        /// Exports a single module's settings to a JSON file.
        /// </summary>
        public bool ExportModuleSettings(string moduleName, string filePath)
        {
            var module = GetModuleByName(moduleName);
            if (module == null)
            {
                Console.WriteLine($"Module '{moduleName}' not found");
                return false;
            }
            return module.Settings.ExportToJsonFile(filePath);
        }

        /// <summary>
        /// Imports a single module's settings from a JSON file.
        /// </summary>
        public bool ImportModuleSettings(string moduleName, string filePath)
        {
            var module = GetModuleByName(moduleName);
            if (module == null)
            {
                Console.WriteLine($"Module '{moduleName}' not found");
                return false;
            }
            return module.Settings.ImportFromJsonFile(filePath);
        }

        /// <summary>
        /// Exports all modules' settings and enabled states to a JSON file.
        /// </summary>
        public bool ExportAllSettings(string filePath)
        {
            return ModuleSettingsStore.ExportAll(_modules, filePath);
        }

        /// <summary>
        /// Imports all modules' settings and enabled states from a JSON file.
        /// </summary>
        public bool ImportAllSettings(string filePath)
        {
            return ModuleSettingsStore.ImportAll(_modules, filePath);
        }

        /// <summary>
        /// Gets a module's settings as a JSON string, or null if the module does not exist.
        /// </summary>
        public string GetModuleSettingsAsJson(string moduleName)
        {
            return GetModuleByName(moduleName)?.Settings.ToJson();
        }

        /// <summary>
        /// Applies settings to a module from a JSON string.
        /// </summary>
        public bool SetModuleSettingsFromJson(string moduleName, string jsonSettings)
        {
            if (string.IsNullOrWhiteSpace(jsonSettings)) return false;
            var module = GetModuleByName(moduleName);
            return module != null && module.Settings.FromJson(jsonSettings);
        }
    }
}
