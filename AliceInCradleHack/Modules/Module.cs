using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliceInCradleHack.Modules
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
        public abstract bool IsEnabled { get; set; }

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

        public List<SettingNode> GetAllLeafNodes()
        {
            var leafNodes = new List<SettingNode>();
            // 叶子节点
            if (Children.Count == 0)
            {
                leafNodes.Add(this);
                return leafNodes;
            }
            // 非叶子节点，递归遍历子节点 | Non-leaf node, recursively traverse child nodes
            foreach (var child in Children.Values)
            {
                leafNodes.AddRange(child.GetAllLeafNodes());
            }
            return leafNodes;
        }

        public string GetPath()
        {
            var segments = new List<string>();
            var currentNode = this;
            while (currentNode != null)
            {
                segments.Add(currentNode.Name);
                currentNode = currentNode.Parent;
            }
            segments.Reverse();
            return string.Join(".", segments);
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
}
