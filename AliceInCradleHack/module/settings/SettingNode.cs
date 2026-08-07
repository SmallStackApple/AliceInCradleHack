using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AliceInCradleHack.module.settings
{
    /// <summary>
    /// A node in a module's settings tree. Leaf nodes hold values; group nodes hold children.
    /// </summary>
    public abstract class SettingNode
    {
        public string Name { get; }
        public string Description { get; }
        internal SettingGroup Parent { get; set; }
        public abstract bool IsLeaf { get; }

        protected SettingNode(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public abstract object GetValue();
        public abstract void SetValue(object value);

        public SettingNode GetNodeByPath(string path)
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

        public object GetValueByPath(string path)
            => GetNodeByPath(path)?.GetValue();

        public bool SetValueByPath(string path, object value)
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

        public Dictionary<string, object> GetAllLeafValues()
        {
            var map = new Dictionary<string, object>();
            CollectLeaves(map);
            return map;
        }

        public List<SettingNode> GetAllLeafNodes()
        {
            var list = new List<SettingNode>();
            CollectLeaves(list);
            return list;
        }

        internal virtual void CollectLeaves(Dictionary<string, object> map)
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
}
