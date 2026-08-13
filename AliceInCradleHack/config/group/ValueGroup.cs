using AliceInCradleHack.utils.client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;

namespace AliceInCradleHack.config.group
{
    /// <summary>
    /// A config node that groups child values. Groups are values themselves and can be nested
    /// to form a tree. Child order is insertion order.
    /// </summary>
    public class ValueGroup : Value, IEnumerable<Value>
    {
        private readonly List<Value> _children = new List<Value>();

        public IReadOnlyList<Value> Children => _children;

        public override ValueType Type => ValueType.Group;

        public ValueGroup(string name, string description = null) : base(name, description) { }

        /// <summary>
        /// Adds a child value. Used by tree building and collection initializers.
        /// </summary>
        public void Add(Value value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (value.Parent != null)
                throw new InvalidOperationException($"'{value.Name}' already belongs to '{value.Parent.GetPath()}'");
            if (string.IsNullOrEmpty(value.Name))
                throw new InvalidOperationException($"Child value of '{GetPath()}' has no name");
            value.Parent = this;
            _children.Add(value);
        }

        /// <summary>
        /// Attaches an existing group as a child of this group.
        /// </summary>
        public T Tree<T>(T group) where T : ValueGroup
        {
            Add(group);
            return group;
        }

        public bool Remove(Value value)
        {
            if (value == null || value.Parent != this) return false;
            value.Parent = null;
            return _children.Remove(value);
        }

        // Factories

        public Value<bool> Boolean(string name, bool defaultValue, string description = null, bool doNotInclude = false)
            => AddValue(new Value<bool>(name, defaultValue, description, ValueType.Boolean) { DoNotInclude = doNotInclude });

        public Value<string> Text(string name, string defaultValue, string description = null)
            => AddValue(new Value<string>(name, defaultValue, description, ValueType.Text));

        public ColorValue Color(string name, string defaultValue, string description = null)
            => AddValue(new ColorValue(name, defaultValue, description));

        public RangedValue<int> Int(string name, int defaultValue, int min, int max, string suffix = "", string description = null)
            => AddValue(new RangedValue<int>(name, defaultValue, min, max, suffix, description));

        public RangedValue<float> Float(string name, float defaultValue, float min, float max, string suffix = "", string description = null)
            => AddValue(new RangedValue<float>(name, defaultValue, min, max, suffix, description));

        public RangedValue<double> Double(string name, double defaultValue, double min, double max, string suffix = "", string description = null)
            => AddValue(new RangedValue<double>(name, defaultValue, min, max, suffix, description));

        public EnumChoiceValue<T> EnumChoice<T>(string name, T defaultValue, string description = null) where T : struct, Enum
            => AddValue(new EnumChoiceValue<T>(name, defaultValue, description));

        public MultiChoiceValue<T> MultiChoice<T>(string name, IEnumerable<T> defaultValue = null, string description = null) where T : struct, Enum
            => AddValue(new MultiChoiceValue<T>(name, defaultValue, description));

        public StringListValue List(string name, IEnumerable<string> defaultValue = null, string description = null)
            => AddValue(new StringListValue(name, defaultValue, description));

        protected T AddValue<T>(T value) where T : Value
        {
            Add(value);
            return value;
        }

        // Tree walking

        public Value GetNodeByPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return this;
            Value current = this;
            foreach (var segment in path.Split('.'))
            {
                if (current is ValueGroup g)
                {
                    Value next = null;
                    foreach (var child in g._children)
                    {
                        if (child.MatchesName(segment)) { next = child; break; }
                    }
                    if (next == null) return null;
                    current = next;
                }
                else return null;
            }
            return current;
        }

        public object GetValueByPath(string path) => GetNodeByPath(path)?.GetValueObject();

        public bool SetValueByPath(string path, object value)
        {
            var node = GetNodeByPath(path);
            if (node == null || node is ValueGroup) return false;
            return node.SetValueObject(value);
        }

        public bool SetByStringPath(string path, string value)
        {
            var node = GetNodeByPath(path);
            if (node == null || node is ValueGroup) return false;
            return node.SetByString(value);
        }

        /// <summary>
        /// All leaf values (non-group, non-hidden) in this tree, in declaration order.
        /// </summary>
        public List<Value> GetAllLeafNodes()
        {
            var list = new List<Value>();
            CollectLeaves(list);
            return list;
        }

        private void CollectLeaves(List<Value> output)
        {
            foreach (var child in _children)
            {
                if (child is ValueGroup g) g.CollectLeaves(output);
                else if (!child.DoNotInclude) output.Add(child);
            }
        }

        public List<Value> CollectValuesRecursively()
        {
            var list = new List<Value>();
            CollectAll(list);
            return list;
        }

        /// <summary>
        /// Propagates a toggle state change down to nested <see cref="ToggleableValueGroup"/>s.
        /// </summary>
        internal void UpdateChildToggleState(bool state)
        {
            foreach (var child in _children)
            {
                if (child is ToggleableValueGroup toggleable)
                {
                    if (toggleable.Enabled) toggleable.PropagateToggled(state);
                }
                else if (child is ValueGroup g)
                {
                    g.UpdateChildToggleState(state);
                }
            }
        }

        private void CollectAll(List<Value> output)
        {
            foreach (var child in _children)
            {
                output.Add(child);
                if (child is ValueGroup g) g.CollectAll(output);
            }
        }

        // Persistence

        public override object GetValueObject() => null;

        public override bool SetValueObject(object value)
        {
            Log.Warn($"'{GetPath()}' is a group, cannot set value");
            return false;
        }

        public override bool SetByString(string value) => SetValueObject(value);

        public override void Restore()
        {
            foreach (var child in _children)
                child.Restore();
        }

        public override JObject ToJToken()
        {
            var array = new JArray();
            foreach (var child in _children)
                array.Add(child.ToJToken());
            return new JObject
            {
                ["name"] = Name,
                ["value"] = array
            };
        }

        /// <summary>
        /// Tolerant deserialization: each child is applied independently; unknown or
        /// malformed entries are skipped with a warning instead of failing the whole file.
        /// Duplicate names are applied in order.
        /// </summary>
        public override void FromJToken(JObject obj)
        {
            if (obj == null) return;
            if (obj["value"] is not JArray storedValues) return;

            var valuesByName = new Dictionary<string, Queue<JObject>>(StringComparer.OrdinalIgnoreCase);
            foreach (var element in storedValues)
            {
                if (element is not JObject valueObj) continue;
                string valueName = valueObj["name"]?.ToString();
                if (string.IsNullOrEmpty(valueName)) continue;
                if (!valuesByName.TryGetValue(valueName, out var queue))
                    valuesByName[valueName] = queue = new Queue<JObject>(1);
                queue.Enqueue(valueObj);
            }

            foreach (var child in _children)
            {
                Queue<JObject> queue = null;
                if (!valuesByName.TryGetValue(child.Name ?? "", out queue) && child.Aliases.Count > 0)
                {
                    foreach (var alias in child.Aliases)
                    {
                        if (valuesByName.TryGetValue(alias, out queue)) break;
                    }
                }
                if (queue == null || queue.Count == 0) continue;

                try
                {
                    child.FromJToken(queue.Dequeue());
                }
                catch (Exception ex)
                {
                    Log.Error($"Unable to deserialize value '{child.GetPath()}'", ex);
                }
            }
        }

        public IEnumerator<Value> GetEnumerator() => _children.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString() => $"ValueGroup(name={Name}, children={_children.Count})";
    }
}
