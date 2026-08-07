using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace AliceInCradleHack.module.settings
{
    /// <summary>
    /// A settings node that groups child nodes.
    /// </summary>
    public class SettingGroup : SettingNode
    {
        private readonly Dictionary<string, SettingNode> _children = new();

        public IReadOnlyDictionary<string, SettingNode> Children => _children;
        public override bool IsLeaf => false;
        public override object GetValue() => null;
        public override void SetValue(object value)
            => throw new InvalidOperationException($"'{GetPath()}' is a group, cannot set value");

        public SettingGroup(string name, string description = null) : base(name, description ?? "") { }

        public void AddChild(SettingNode child)
        {
            if (child.Parent != null)
                throw new InvalidOperationException($"'{child.Name}' already belongs to '{child.Parent.GetPath()}'");
            if (_children.ContainsKey(child.Name))
                throw new ArgumentException($"Child '{child.Name}' already exists in '{GetPath()}'");
            child.Parent = this;
            _children[child.Name] = child;
        }

        internal override void CollectLeaves(Dictionary<string, object> map)
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
}
