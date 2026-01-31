using AliceInCradleHack.Modules;
using AliceInCradleHack.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Module = AliceInCradleHack.Modules.Module;

namespace AliceInCradleHack
{
    /// <summary>
    /// 模块管理器（单例模式） | Module Manager (Singleton Pattern)
    /// 负责模块的注册、初始化、启用/禁用等管理操作 | Responsible for module registration, initialization, enable/disable and other management operations
    /// </summary>
    public class ModuleManager
    {
        // 线程安全的模块字典 | Thread-safe module dictionary
        private readonly ConcurrentDictionary<string, Module> _modules = new ConcurrentDictionary<string, Module>();

        // 按键绑定的模块字典 | Key-bind modules dictionary
        private readonly ConcurrentDictionary<string, Module> _keyBindModules = new ConcurrentDictionary<string, Module>();

        /// <summary>
        /// 懒加载单例（线程安全） | Lazy singleton (thread-safe)
        /// </summary>
        private static readonly Lazy<ModuleManager> _lazyInstance = new Lazy<ModuleManager>(() => new ModuleManager());

        /// <summary>
        /// 模块管理器单例实例 | Module Manager singleton instance
        /// </summary>
        public static ModuleManager Instance { get; } = _lazyInstance.Value;

        /// <summary>
        /// 初始化内置模块 | Initialize built-in modules
        /// 注册所有内置模块并执行初始化 | Register all built-in modules and execute initialization
        /// </summary>
        public void Initialize()
        {
            var builtInModules = new List<Module>
            {
                new ModuleMosaicRemove(),
                new ModuleDiscordRPC(),
                new ModuleKillSound(),
                new ModuleCritical()
                // 在此处添加其他模块实例 | Add other module instances here
            };

            foreach (var module in builtInModules)
            {
                RegisterModule(module);
            }

            
        }

        /// <summary>
        /// 从程序集文件加载并注册模块 | Load and register modules from assembly file
        /// </summary>
        /// <param name="assemblyPath">程序集文件路径 | Assembly file path</param>
        public void RegisterModuleFromAssemblyFile(string assemblyPath)
        {
            // 参数校验 | Parameter validation
            if (string.IsNullOrWhiteSpace(assemblyPath))
            {
                throw new ArgumentNullException(nameof(assemblyPath), "Assembly file path cannot be null or empty");
            }

            if (!File.Exists(assemblyPath))
            {
                throw new FileNotFoundException("Assembly file not found", assemblyPath);
            }

            if (!assemblyPath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Invalid assembly file type. Only .dll are supported.", nameof(assemblyPath));
            }

            try
            {

                // 加载目录下的lib中的依赖项 | Load dependencies from lib folder in the same directory
                if (Directory.Exists(Path.Combine(Path.GetDirectoryName(assemblyPath), "lib")))
                {
                    AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
                    {
                        var assemblyName = new AssemblyName(args.Name).Name + ".dll";
                        var libPath = Path.Combine(Path.GetDirectoryName(assemblyPath), "lib", assemblyName);
                        if (File.Exists(libPath))
                        {
                            return Assembly.LoadFrom(libPath);
                        }
                        return null;
                    };
                }

                // 加载程序集 | Load assembly
                var assembly = Assembly.LoadFrom(assemblyPath);

                // 获取所有非抽象的Module子类 | Get all non-abstract subclasses of Module
                var moduleTypes = assembly.GetTypes()
                    .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(Module)));

                foreach (var type in moduleTypes)
                {
                    try
                    {
                        // 创建模块实例并注册 | Create module instance and register
                        var moduleInstance = (Module)Activator.CreateInstance(type);
                        RegisterModule(moduleInstance);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to create module instance {type.FullName}: {ex.Message}");
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"Assembly file not found {assemblyPath}: {ex.Message}");
            }
            catch (BadImageFormatException ex)
            {
                Console.WriteLine($"Invalid assembly format {assemblyPath}: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load modules from assembly {assemblyPath}: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取所有已注册的模块 | Get all registered modules
        /// </summary>
        /// <returns>模块集合 | Collection of modules</returns>
        public IEnumerable<Module> GetAllModules() => _modules.Values;

        /// <summary>
        /// 注册模块（自动执行初始化） | Register module (automatically execute initialization)
        /// </summary>
        /// <param name="module">待注册的模块实例 | Module instance to register</param>
        /// <exception cref="ArgumentNullException">模块实例为空时抛出 | Thrown when module instance is null</exception>
        public void RegisterModule(Module module)
        {
            // 参数校验 | Parameter validation
            if (module == null)
            {
                throw new ArgumentNullException(nameof(module), "Module instance cannot be null");
            }

            // 线程安全的添加操作，避免重复注册 | Thread-safe add operation to avoid duplicate registration
            if (_modules.TryAdd(module.Name, module))
            {
                try
                {
                    module.Initialize();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to initialize module {module.Name}: {ex.Message}");
                    // 初始化失败时移除模块 | Remove module if initialization fails
                    _modules.TryRemove(module.Name, out _);
                }
            }
            else
            {
                Console.WriteLine($"Module already exists, skip registration {module.Name}");
            }
        }

        /// <summary>
        /// 启用指定模块 | Enable specified module
        /// </summary>
        /// <param name="moduleName">模块名称 | Module name</param>
        public void EnableModule(string moduleName)
        {
            if (string.IsNullOrWhiteSpace(moduleName)) return;

            if (_modules.TryGetValue(moduleName, out var module) && !module.IsEnabled)
            {
                try
                {
                    module.Enable();
                    module.IsEnabled = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to enable module {moduleName}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 禁用指定模块 | Disable specified module
        /// </summary>
        /// <param name="moduleName">模块名称 | Module name</param>
        public void DisableModule(string moduleName)
        {
            if (string.IsNullOrWhiteSpace(moduleName)) return;

            if (_modules.TryGetValue(moduleName, out var module) && module.IsEnabled)
            {
                try
                {
                    module.Disable();
                    module.IsEnabled = false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to disable module {moduleName}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 切换指定模块的启用状态 | Toggle enable status of specified module
        /// </summary>
        /// <param name="moduleName">模块名称 | Module name</param>
        public void ToggleModule(string moduleName)
        {
            if (string.IsNullOrWhiteSpace(moduleName)) return;

            if (_modules.TryGetValue(moduleName, out var module))
            {
                if (module.IsEnabled)
                {
                    DisableModule(moduleName);
                    Notification.ShowNotification($"Disabled {module.Name}", Notification.NotificationType.ALERT);
                }
                else
                {
                    EnableModule(moduleName);
                    Notification.ShowNotification($"Enabled {module.Name}", Notification.NotificationType.ALERT);
                }
            }
        }

        /// <summary>
        /// 根据名称获取模块 | Get module by name
        /// </summary>
        /// <param name="moduleName">模块名称 | Module name</param>
        /// <returns>模块实例（不存在则返回null） | Module instance (null if not exists)</returns>
        public Module GetModuleByName(string moduleName)
        {
            if (string.IsNullOrWhiteSpace(moduleName)) return null;
            _modules.TryGetValue(moduleName, out var module);
            return module;
        }

        /// <summary>
        /// 根据分类获取模块 | Get modules by category
        /// </summary>
        /// <param name="category">模块分类 | Module category</param>
        /// <returns>指定分类的模块集合 | Collection of modules in specified category</returns>
        public IEnumerable<Module> GetModulesByCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category)) return Enumerable.Empty<Module>();
            return _modules.Values.Where(m => m.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 获取所有已启用的模块 | Get all enabled modules
        /// </summary>
        /// <returns>已启用模块集合 | Collection of enabled modules</returns>
        public IEnumerable<Module> GetEnabledModules() => _modules.Values.Where(m => m.IsEnabled);

        /// <summary>
        /// 获取所有已禁用的模块 | Get all disabled modules
        /// </summary>
        /// <returns>已禁用模块集合 | Collection of disabled modules</returns>
        public IEnumerable<Module> GetDisabledModules() => _modules.Values.Where(m => !m.IsEnabled);

        /// <summary>
        /// 检查模块是否启用 | Check if module is enabled
        /// </summary>
        /// <param name="moduleName">模块名称 | Module name</param>
        /// <returns>启用状态（不存在则返回false） | Enable status (false if not exists)</returns>
        public bool IsModuleEnabled(string moduleName)
        {
            if (string.IsNullOrWhiteSpace(moduleName)) return false;
            return _modules.TryGetValue(moduleName, out var module) && module.IsEnabled;
        }

        /// <summary>
        /// 获取模块所有叶子设置的路径（如 "Display/Window/Width"）
        /// </summary>
        /// <param name="moduleName">模块名称</param>
        /// <returns>所有叶子节点路径</returns>
        public string[] GetSettingPaths(string moduleName)
        {
            if (!_modules.ContainsKey(moduleName))
            {
                return Array.Empty<string>();
            }

            var module = _modules[moduleName];
            var allLeafValues = module.Settings.GetAllLeafValues();
            return allLeafValues.Keys.ToArray();
        }

        /// <summary>
        /// 通过路径获取模块设置值
        /// </summary>
        /// <param name="moduleName">模块名称</param>
        /// <param name="settingPath">设置路径（如 "Display/Window/Width"）</param>
        /// <returns>设置值（不存在返回null）</returns>
        public object GetSettingValue(string moduleName, string settingPath)
        {
            if (!_modules.ContainsKey(moduleName))
            {
                return null;
            }

            var module = _modules[moduleName];
            return module.Settings.GetValueByPath(settingPath);
        }

        /// <summary>
        /// 通过路径设置模块设置值
        /// </summary>
        /// <param name="moduleName">模块名称</param>
        /// <param name="settingPath">设置路径</param>
        /// <param name="value">新值</param>
        /// <returns>是否设置成功</returns>
        public bool SetSettingValue(string moduleName, string settingPath, object value)
        {
            if (!_modules.ContainsKey(moduleName))
            {
                return false;
            }

            var module = _modules[moduleName];
            return module.Settings.SetValueByPath(settingPath, value);
        }

        /// <summary>
        /// 获取设置节点的详细信息（描述、类型、是否可编辑等）
        /// </summary>
        /// <param name="moduleName">模块名称</param>
        /// <param name="settingPath">设置路径</param>
        /// <returns>SettingNode（不存在返回null）</returns>
        public SettingNode GetSettingNode(string moduleName, string settingPath)
        {
            if (!_modules.ContainsKey(moduleName))
            {
                return null;
            }

            var module = _modules[moduleName];
            return module.Settings.GetNodeByPath(settingPath);
        }
    }
}