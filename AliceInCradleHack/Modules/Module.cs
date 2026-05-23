using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AliceInCradleHack.Modules
{
    public abstract class Module
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string Author { get; }
        public abstract string Version { get; }
        public virtual bool IsEnabled { get; set; } = false;
        public virtual string Category { get; } = "General";
        public abstract SettingNode Settings { get; }
        public abstract void Initialize();
        public abstract void Enable();
        public abstract void Disable();
    }

    public abstract class SettingNode
    {
        public string Name { get; }
        public string Description { get; }
        internal SettingGroup? Parent { get; set; }
        public abstract bool IsLeaf { get; }

        protected SettingNode(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public abstract object? GetValue();
        public abstract void SetValue(object? value);

        public SettingNode? GetNodeByPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return this;
            var cur = this;
            foreach (var seg in path.Split('.'))
            {
                if (cur is SettingGroup g && g.Children.TryGetValue(seg, out var next))
                    cur = next;
                else
                    return null;
            }
            return cur;
        }

        public object? GetValueByPath(string path)
            => GetNodeByPath(path)?.GetValue();

        public bool SetValueByPath(string path, object? value)
        {
            var node = GetNodeByPath(path);
            if (node == null || !node.IsLeaf) return false;
            try { node.SetValue(value); return true; }
            catch { return false; }
        }

        public string GetPath()
        {
            var segs = new List<string>();
            for (var n = this; n?.Parent != null; n = n.Parent)
                segs.Add(n.Name);
            segs.Reverse();
            return string.Join(".", segs);
        }

        public Dictionary<string, object?> GetAllLeafValues()
        {
            var map = new Dictionary<string, object?>();
            CollectLeaves(map);
            return map;
        }

        public List<SettingNode> GetAllLeafNodes()
        {
            var list = new List<SettingNode>();
            CollectLeaves(list);
            return list;
        }

        internal virtual void CollectLeaves(Dictionary<string, object?> map)
        {
            if (IsLeaf) map[GetPath()] = GetValue();
        }

        internal virtual void CollectLeaves(List<SettingNode> list)
        {
            if (IsLeaf) list.Add(this);
        }

        public abstract JToken ToJToken();
        public abstract void FromJToken(JToken token);

        public string ToJson()
        {
            return ToJToken().ToString(Formatting.Indented);
        }

        public bool FromJson(string json)
        {
            try
            {
                FromJToken(JToken.Parse(json));
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JSON error at '{GetPath()}': {ex.Message}");
                return false;
            }
        }

        public bool ExportToJsonFile(string path)
        {
            try
            {
                File.WriteAllText(path, ToJson(), Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Export failed: {ex.Message}");
                return false;
            }
        }

        public bool ImportFromJsonFile(string path)
        {
            try
            {
                if (!File.Exists(path)) return false;
                return FromJson(File.ReadAllText(path, Encoding.UTF8));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Import failed: {ex.Message}");
                return false;
            }
        }
    }

    public class SettingGroup : SettingNode
    {
        private readonly Dictionary<string, SettingNode> _children = new();

        public IReadOnlyDictionary<string, SettingNode> Children => _children;
        public override bool IsLeaf => false;
        public override object? GetValue() => null;
        public override void SetValue(object? value)
            => throw new InvalidOperationException($"'{GetPath()}' is a group, cannot set value");

        public SettingGroup(string name, string? description = null) : base(name, description ?? "") { }

        public void AddChild(SettingNode child)
        {
            if (child.Parent != null)
                throw new InvalidOperationException($"'{child.Name}' already belongs to '{child.Parent.GetPath()}'");
            if (_children.ContainsKey(child.Name))
                throw new ArgumentException($"Child '{child.Name}' already exists in '{GetPath()}'");
            child.Parent = this;
            _children[child.Name] = child;
        }

        internal override void CollectLeaves(Dictionary<string, object?> map)
        {
            foreach (var c in _children.Values) c.CollectLeaves(map);
        }

        internal override void CollectLeaves(List<SettingNode> list)
        {
            foreach (var c in _children.Values) c.CollectLeaves(list);
        }

        public override JToken ToJToken()
        {
            var obj = new JObject();
            foreach (var kvp in _children)
                obj[kvp.Key] = kvp.Value.ToJToken();
            return obj;
        }

        public override void FromJToken(JToken token)
        {
            if (token is not JObject obj)
                throw new InvalidOperationException($"Expected JSON object for group '{GetPath()}'");
            foreach (var prop in obj.Properties())
            {
                if (!_children.TryGetValue(prop.Name, out var child))
                {
                    Console.WriteLine($"Unknown key '{prop.Name}' in '{GetPath()}'");
                    continue;
                }
                child.FromJToken(prop.Value);
            }
        }
    }

    public class Setting<T> : SettingNode
    {
        private T _value;

        public T Value
        {
            get => _value;
            set
            {
                if (!IsEditable) throw new InvalidOperationException($"'{GetPath()}' is read-only");
                _value = value;
            }
        }

        public bool IsEditable { get; }
        public override bool IsLeaf => true;

        public Setting(string name, string description, T defaultValue, bool isEditable = true)
            : base(name, description)
        {
            _value = defaultValue;
            IsEditable = isEditable;
        }

        public override object? GetValue() => _value;
        public override void SetValue(object? value)
            => _value = (T)Convert.ChangeType(value, typeof(T));

        public override JToken ToJToken() => JToken.FromObject(_value);
        public override void FromJToken(JToken token) => _value = token.ToObject<T>();
    }

    public class SettingBuilder
    {
        private readonly SettingGroup _root;
        private SettingGroup _current;

        public SettingBuilder(string name = "Root", string? description = null)
        {
            _root = new SettingGroup(name, description);
            _current = _root;
        }

        public SettingBuilder Group(string name, string? description = null)
        {
            _current = _current.Children.TryGetValue(name, out var existing)
                ? (SettingGroup)existing
                : AddChild(new SettingGroup(name, description));
            return this;
        }

        public SettingBuilder Add<T>(string name, string description, T defaultValue, bool isEditable = true)
        {
            _current.AddChild(new Setting<T>(name, description, defaultValue, isEditable));
            return this;
        }

        public SettingBuilder Back()
        {
            if (_current.Parent is SettingGroup p) _current = p;
            return this;
        }

        public SettingBuilder Reset()
        {
            _current = _root;
            return this;
        }

        public SettingGroup Build() => _root;

        public static SettingGroup Create(Action<SettingBuilder> buildAction)
        {
            var b = new SettingBuilder();
            buildAction(b);
            return b.Build();
        }

        private SettingGroup AddChild(SettingGroup child)
        {
            _current.AddChild(child);
            return child;
        }
    }
}
