using AliceInCradleHack.utils.client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace AliceInCradleHack.config
{
    /// <summary>
    /// A node in the config tree. Leaf values derive from <see cref="Value{T}"/>;
    /// groups derive from <see cref="group.ValueGroup"/>.
    /// </summary>
    public abstract class Value
    {
        public string Name { get; internal set; }
        public string Description { get; set; }
        public List<string> Aliases { get; } = new List<string>();

        /// <summary>
        /// The group this value belongs to, or null when it is a root config.
        /// </summary>
        public group.ValueGroup Parent { get; internal set; }

        public abstract ValueType Type { get; }

        /// <summary>
        /// If true, the value can never be changed and always keeps its default.
        /// </summary>
        public bool IsImmutable { get; set; }

        /// <summary>
        /// If true, the value is hidden from listings (WebUI, commands) but still serialized.
        /// </summary>
        public bool DoNotInclude { get; set; }

        public bool IsEditable => !IsImmutable;

        protected Value(string name, string description = null)
        {
            Name = name;
            Description = description ?? "";
        }

        public abstract object GetValueObject();
        public abstract bool SetValueObject(object value);
        public abstract bool SetByString(string value);
        public abstract void Restore();

        /// <summary>
        /// Dot-separated path from the root config, excluding the root's own name.
        /// </summary>
        public string GetPath()
        {
            var segments = new List<string>();
            for (var node = this; node?.Parent != null; node = node.Parent)
                segments.Add(node.Name);
            segments.Reverse();
            return string.Join(".", segments);
        }

        public abstract JObject ToJToken();

        /// <summary>
        /// Applies a serialized json object of the shape { "name": ..., "value": ... }.
        /// Implementations must be tolerant: unknown or malformed content is skipped, not thrown.
        /// </summary>
        public abstract void FromJToken(JObject obj);

        /// <summary>
        /// Whether this node matches the given serialized name (by name or alias).
        /// </summary>
        public bool MatchesName(string serializedName)
        {
            if (string.Equals(Name, serializedName, StringComparison.OrdinalIgnoreCase)) return true;
            foreach (var alias in Aliases)
                if (string.Equals(alias, serializedName, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        public override string ToString() => $"{GetType().Name}(name={Name}, type={Type})";
    }

    /// <summary>
    /// A leaf config value with a typed payload, change interception and change notification.
    /// </summary>
    public class Value<T> : Value
    {
        protected T _inner;
        private readonly T _defaultValue;

        private readonly List<Func<T, T>> _interceptors = new List<Func<T, T>>();
        private readonly List<Action<T>> _changedListeners = new List<Action<T>>();

        public override ValueType Type { get; }

        public Value(T defaultValue, string description = null, ValueType type = ValueType.Invalid)
            : base(null, description)
        {
            _inner = defaultValue;
            _defaultValue = defaultValue;
            Type = type == ValueType.Invalid ? GuessType() : type;
        }

        public Value(string name, T defaultValue, string description, ValueType type = ValueType.Invalid)
            : base(name, description)
        {
            _inner = defaultValue;
            _defaultValue = defaultValue;
            Type = type == ValueType.Invalid ? GuessType() : type;
        }

        protected static ValueType GuessType()
        {
            var t = typeof(T);
            if (t == typeof(bool)) return ValueType.Boolean;
            if (t == typeof(int)) return ValueType.Int;
            if (t == typeof(float)) return ValueType.Float;
            if (t == typeof(double)) return ValueType.Double;
            if (t == typeof(string)) return ValueType.Text;
            return ValueType.Invalid;
        }

        public T Get() => _inner;

        public virtual void Set(T value)
        {
            if (EqualityComparer<T>.Default.Equals(value, _inner)) return;

            T current = value;
            try
            {
                foreach (var interceptor in _interceptors)
                    current = interceptor(current);

                if (IsImmutable) return;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to set '{GetPath()}' from '{_inner}' to '{value}'", ex);
                return;
            }

            _inner = current;
            foreach (var listener in _changedListeners)
            {
                try { listener(current); }
                catch (Exception ex) { Log.Error($"OnChanged listener of '{GetPath()}' failed", ex); }
            }
        }

        /// <summary>
        /// Adds an interceptor that can transform (or veto, by throwing) an incoming value before it is applied.
        /// </summary>
        public Value<T> OnChange(Func<T, T> interceptor)
        {
            _interceptors.Add(interceptor);
            return this;
        }

        /// <summary>
        /// Adds a listener that is notified after the value has changed.
        /// </summary>
        public Value<T> OnChanged(Action<T> listener)
        {
            _changedListeners.Add(listener);
            return this;
        }

        public override void Restore() => Set(_defaultValue);

        public override object GetValueObject() => _inner;

        public override bool SetValueObject(object value)
        {
            try
            {
                if (value is string s) return SetByString(s);
                if (value is T typed)
                {
                    Set(typed);
                    return true;
                }
                Set((T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture));
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to set '{GetPath()}' to '{value}'", ex);
                return false;
            }
        }

        public override bool SetByString(string value)
        {
            try
            {
                Set(Parse(value));
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to parse '{value}' for '{GetPath()}'", ex);
                return false;
            }
        }

        protected virtual T Parse(string value)
        {
            var t = typeof(T);
            if (t == typeof(string)) return (T)(object)value;
            if (t == typeof(bool)) return (T)(object)bool.Parse(value);
            return (T)Convert.ChangeType(value, t, CultureInfo.InvariantCulture);
        }

        public override JObject ToJToken() => new JObject
        {
            ["name"] = Name,
            ["value"] = _inner == null ? JValue.CreateNull() : JToken.FromObject(_inner)
        };

        public override void FromJToken(JObject obj)
        {
            if (obj == null) return;
            var token = obj["value"];
            if (token == null || token.Type == JTokenType.Null) return;
            try
            {
                Set(token.ToObject<T>());
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to deserialize value '{GetPath()}'", ex);
            }
        }

        public static implicit operator T(Value<T> value) => value.Get();

        public override string ToString() => $"Value(name={Name}, value={_inner}, type={Type})";
    }
}
