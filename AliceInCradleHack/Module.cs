using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AliceInCradleHack
{
    /// <summary>
    /// 模块抽象基类 | Abstract base class for all modules
    /// 所有功能模块需继承此类并实现抽象成员 | All functional modules must inherit this class and implement abstract members
    /// </summary>
    public abstract class Module
    {
        /// <summary>
        /// 模块名称（唯一标识） | Module name (unique identifier)
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// 模块描述 | Module description
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// 模块作者 | Module author
        /// </summary>
        public abstract string Author { get; }

        /// <summary>
        /// 模块版本 | Module version
        /// </summary>
        public abstract string Version { get; }

        /// <summary>
        /// 模块启用状态 | Module enable status
        /// </summary>
        public abstract bool IsEnabled { get;set; }

        /// <summary>
        /// 模块分类（默认：General） | Module category (Default: General)
        /// </summary>
        public virtual string Category { get; } = "General";

        /// <summary>
        /// 模块配置项 | Module settings
        /// Key: 配置名称 | Key: Setting name
        /// Value: 配置值 | Value: Setting value
        /// </summary>
        public abstract SettingNode Settings { get; }

        /// <summary>
        /// 初始化模块（请勿阻塞此方法） | Initialize module (Do not block this method)
        /// 用于执行模块启动前的初始化逻辑 | Used to execute initialization logic before module startup
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// 启用模块（请勿阻塞此方法） | Enable module (Do not block this method)
        /// 用于执行模块启用时的逻辑 | Used to execute logic when module is enabled
        /// </summary>
        public abstract void Enable();

        /// <summary>
        /// 禁用模块（请勿阻塞此方法） | Disable module (Do not block this method)
        /// 用于执行模块禁用时的逻辑 | Used to execute logic when module is disabled
        /// </summary>
        public abstract void Disable();
    }

    /// <summary>
    /// 树状设置节点，支持多级嵌套的设置结构 | Tree-structured setting node, supports multi-level nested setting structure
    /// </summary>
    public class SettingNode
    {
        /// <summary>
        /// 节点名称（唯一标识） | Node name (unique identifier)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 节点描述（用于UI展示） | Node description (for UI display)
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 节点值（叶子节点有效） | Node value (valid for leaf nodes)
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// 值的类型（用于序列化/反序列化和类型校验） | Value type (used for serialization/deserialization and type validation)
        /// </summary>
        public Type ValueType { get; set; }

        /// <summary>
        /// 是否为可编辑节点（叶子节点有效） | Whether the node is editable (valid for leaf nodes)
        /// </summary>
        public bool IsEditable { get; set; } = true;

        /// <summary>
        /// 子节点（非叶子节点有效） | Child nodes (valid for non-leaf nodes)
        /// </summary>
        public Dictionary<string, SettingNode> Children { get; set; } = new Dictionary<string, SettingNode>();

        /// <summary>
        /// 父节点（用于反向查找） | Parent node (used for reverse lookup)
        /// </summary>
        public SettingNode Parent { get; private set; }

        /// <summary>
        /// 构造根节点 | Construct root node
        /// </summary>
        public SettingNode() : this("Root", "Root setting node") { }

        /// <summary>
        /// 构造子节点 | Construct child node
        /// </summary>
        /// <param name="name">节点名称 | Node name</param>
        /// <param name="description">节点描述 | Node description</param>
        public SettingNode(string name, string description)
        {
            Name = name;
            Description = description;
            ValueType = null; // 非叶子节点默认无类型 | Non-leaf nodes default to no type
        }

        /// <summary>
        /// 构造叶子节点（带值） | Construct leaf node (with value)
        /// </summary>
        /// <param name="name">节点名称 | Node name</param>
        /// <param name="description">节点描述 | Node description</param>
        /// <param name="value">节点值 | Node value</param>
        /// <param name="isEditable">是否可编辑 | Whether editable</param>
        public SettingNode(string name, string description, object value, bool isEditable = true)
        {
            Name = name;
            Description = description;
            Value = value;
            ValueType = value?.GetType() ?? typeof(object);
            IsEditable = isEditable;
        }

        /// <summary>
        /// 添加子节点 | Add child node
        /// </summary>
        /// <param name="childNode">子节点 | Child node</param>
        public void AddChild(SettingNode childNode)
        {
            if (!Children.ContainsKey(childNode.Name))
            {
                childNode.Parent = this;
                Children.Add(childNode.Name, childNode);
            }
        }

        /// <summary>
        /// 通过路径获取节点（如 "Display.Window.Width"） | Get node by path (e.g., "Display.Window.Width")
        /// </summary>
        /// <param name="path">层级路径，使用 . 分隔 | Hierarchical path, separated by .</param>
        /// <returns>目标节点（不存在返回null） | Target node (null if not found)</returns>
        public SettingNode GetNodeByPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return this;

            var pathSegments = path.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            var currentNode = this;

            foreach (var segment in pathSegments)
            {
                if (!currentNode.Children.TryGetValue(segment, out var nextNode))
                {
                    return null;
                }
                currentNode = nextNode;
            }

            return currentNode;
        }

        /// <summary>
        /// 通过路径设置节点值（仅叶子节点有效） | Set node value by path (valid for leaf nodes only)
        /// </summary>
        /// <param name="path">层级路径 | Hierarchical path</param>
        /// <param name="value">新值 | New value</param>
        /// <returns>是否设置成功 | Whether setting was successful</returns>
        public bool SetValueByPath(string path, object value)
        {
            var targetNode = GetNodeByPath(path);
            if (targetNode == null || targetNode.Children.Count > 0 || !targetNode.IsEditable)
            {
                return false;
            }

            // 类型校验 | Type check
            if (value != null && targetNode.ValueType != null && targetNode.ValueType != value.GetType())
            {
                try
                {
                    // 尝试类型转换 | Attempt type conversion
                    value = Convert.ChangeType(value, targetNode.ValueType);
                }
                catch
                {
                    return false;
                }
            }

            targetNode.Value = value;
            return true;
        }

        /// <summary>
        /// 通过路径获取节点值 | Get node value by path
        /// </summary>
        /// <param name="path">层级路径 | Hierarchical path</param>
        /// <returns>节点值（不存在/非叶子节点返回null） | Node value (null if not found/non-leaf node)</returns>
        public object GetValueByPath(string path)
        {
            var targetNode = GetNodeByPath(path);
            return targetNode?.Children.Count == 0 ? targetNode.Value : null;
        }

        /// <summary>
        /// 递归获取所有叶子节点的路径和值 | Recursively get all leaf nodes' paths and values
        /// </summary>
        /// <param name="prefix">路径前缀 | Path prefix</param>
        /// <returns>键值对（路径: 值） | Key-value pairs (path: value)</returns>
        public Dictionary<string, object> GetAllLeafValues(string prefix = "")
        {
            var result = new Dictionary<string, object>();
            var currentPath = string.IsNullOrEmpty(prefix) ? Name : $"{prefix}.{Name}";

            // 叶子节点
            if (Children.Count == 0)
            {
                result.Add(currentPath, Value);
                return result;
            }

            // 非叶子节点，递归遍历子节点 | Non-leaf node, recursively traverse child nodes
            foreach (var child in Children.Values)
            {
                var childValues = child.GetAllLeafValues(currentPath);
                foreach (var kvp in childValues)
                {
                    result.Add(kvp.Key, kvp.Value);
                }
            }

            return result;
        }
    }

    /// <summary>
    /// 树状设置构建器（简化节点创建） | Setting Node Builder (Simplify node creation)
    /// </summary>
    public class SettingBuilder
    {
        private readonly SettingNode _rootNode;
        private SettingNode _currentNode;

        /// <summary>
        /// 初始化构建器（默认创建根节点）| Initialize builder (default create root node)
        /// </summary>
        public SettingBuilder()
        {
            _rootNode = new SettingNode();
            _currentNode = _rootNode;
        }

        /// <summary>
        /// 进入指定层级（创建/切换到父节点）
        /// Enter specified level (create/switch to parent node)
        /// </summary>
        /// <param name="name">节点名称 | Node name</param>
        /// <param name="description">节点描述 | Node description</param>
        /// <returns>构建器本身（链式调用） | The builder itself (for chaining)</returns>
        public SettingBuilder Group(string name, string description)
        {
            // 如果当前层级已存在该子节点，直接切换；否则创建新节点
            if (!_currentNode.Children.TryGetValue(name, out var groupNode))
            {
                groupNode = new SettingNode(name, description);
                _currentNode.AddChild(groupNode);
            }
            _currentNode = groupNode;
            return this;
        }

        /// <summary>
        /// 添加叶子节点（配置项） | Add leaf node (setting item)
        /// </summary>
        /// <param name="name">配置项名称 | Setting item name</param>
        /// <param name="description">配置项描述 | Setting item description</param>
        /// <param name="value">默认值 | Default value</param>
        /// <param name="isEditable">是否可编辑 | Is editable</param>
        /// <returns>构建器本身（链式调用） | The builder itself (for chaining)</returns>
        public SettingBuilder Add(string name, string description, object value, bool isEditable = true)
        {
            var leafNode = new SettingNode(name, description, value, isEditable);
            _currentNode.AddChild(leafNode);
            return this;
        }

        /// <summary>
        /// 返回上一级节点 | Return to parent node
        /// </summary>
        /// <returns>构建器本身（链式调用） | The builder itself (for chaining)</returns>
        public SettingBuilder Back()
        {
            if (_currentNode.Parent != null)
            {
                _currentNode = _currentNode.Parent;
            }
            return this;
        }

        /// <summary>
        /// 重置到根节点 | Reset to root node
        /// </summary>
        /// <returns>构建器本身（链式调用） | The builder itself (for chaining)</returns>
        public SettingBuilder Reset()
        {
            _currentNode = _rootNode;
            return this;
        }

        /// <summary>
        /// 构建完成，返回根节点 | Build completed, return root node
        /// </summary>
        /// <returns>树状设置根节点 | Root node of the tree-structured settings</returns>
        public SettingNode Build()
        {
            return _rootNode;
        }

        // 静态快捷方法：快速创建根节点 + 一级配置项 | Static shortcut method: quickly create root node + first-level configuration items
        public static SettingNode Create(Action<SettingBuilder> buildAction)
        {
            var builder = new SettingBuilder();
            buildAction(builder);
            return builder.Build();
        }
    }


    /// <summary>
    /// 模块管理器（单例模式） | Module Manager (Singleton Pattern)
    /// 负责模块的注册、初始化、启用/禁用等管理操作 | Responsible for module registration, initialization, enable/disable and other management operations
    /// </summary>
    public class ModuleManager
    {
        // 线程安全的模块字典 | Thread-safe module dictionary
        private readonly ConcurrentDictionary<string, Module> _modules = new ConcurrentDictionary<string, Module>();

        /// <summary>
        /// 懒加载单例（线程安全） | Lazy singleton (thread-safe)
        /// </summary>
        private static readonly Lazy<ModuleManager> _lazyInstance = new Lazy<ModuleManager>(() => new ModuleManager());

        /// <summary>
        /// 模块管理器单例实例 | Module Manager singleton instance
        /// </summary>
        public static ModuleManager Instance => _lazyInstance.Value;

        /// <summary>
        /// 初始化内置模块 | Initialize built-in modules
        /// 注册所有内置模块并执行初始化 | Register all built-in modules and execute initialization
        /// </summary>
        public void Initialize()
        {
            var builtInModules = new List<Module>
            {
                new Modules.ModuleMosaicRemove(),
                new Modules.ModuleDiscordRPC(),
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

            try
            {
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
                }
                else
                {
                    EnableModule(moduleName);
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