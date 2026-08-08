using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AliceInCradleHack.config
{
    /// <summary>
    /// A value that selects exactly one option of an enum, serialized by option name.
    /// </summary>
    public class EnumChoiceValue<T> : Value<T> where T : struct, Enum
    {
        public IReadOnlyList<T> Choices { get; } = (T[])Enum.GetValues(typeof(T));

        public EnumChoiceValue(T defaultValue, string description = null)
            : base(defaultValue, description, ValueType.EnumChoice) { }

        public EnumChoiceValue(string name, T defaultValue, string description = null)
            : base(name, defaultValue, description, ValueType.EnumChoice) { }

        protected override T Parse(string value)
        {
            return (T)Enum.Parse(typeof(T), value, ignoreCase: true);
        }

        public override JObject ToJToken() => new JObject
        {
            ["name"] = Name,
            ["value"] = _inner.ToString()
        };

        public override void FromJToken(JObject obj)
        {
            if (obj == null) return;
            var token = obj["value"];
            if (token == null || token.Type == JTokenType.Null) return;
            try
            {
                Set(token.Type == JTokenType.String
                    ? (T)Enum.Parse(typeof(T), token.ToString(), ignoreCase: true)
                    : token.ToObject<T>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to deserialize enum choice '{GetPath()}': {ex.Message}");
            }
        }
    }

    /// <summary>
    /// A value that selects multiple options of an enum, serialized as an array of option names.
    /// </summary>
    public class MultiChoiceValue<T> : Value<HashSet<T>> where T : struct, Enum
    {
        public IReadOnlyList<T> Choices { get; } = (T[])Enum.GetValues(typeof(T));

        public MultiChoiceValue(IEnumerable<T> defaultValue = null, string description = null)
            : base(defaultValue == null ? new HashSet<T>() : new HashSet<T>(defaultValue), description, ValueType.MultiChoice) { }

        public MultiChoiceValue(string name, IEnumerable<T> defaultValue = null, string description = null)
            : base(name, defaultValue == null ? new HashSet<T>() : new HashSet<T>(defaultValue), description, ValueType.MultiChoice) { }

        public bool Contains(T option) => _inner.Contains(option);

        public void Enable(T option) => Set(new HashSet<T>(_inner) { option });

        public void Disable(T option)
        {
            var copy = new HashSet<T>(_inner);
            copy.Remove(option);
            Set(copy);
        }

        public override bool SetByString(string value)
        {
            try
            {
                var set = new HashSet<T>(
                    value.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                         .Select(s => (T)Enum.Parse(typeof(T), s.Trim(), ignoreCase: true)));
                Set(set);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to parse '{value}' for '{GetPath()}': {ex.Message}");
                return false;
            }
        }

        public override bool SetValueObject(object value)
        {
            if (value is string s) return SetByString(s);
            if (value is IEnumerable<T> typed)
            {
                Set(new HashSet<T>(typed));
                return true;
            }
            return base.SetValueObject(value);
        }

        public override JObject ToJToken() => new JObject
        {
            ["name"] = Name,
            ["value"] = new JArray(_inner.Select(c => c.ToString()))
        };

        public override void FromJToken(JObject obj)
        {
            if (obj == null) return;
            var token = obj["value"];
            if (token is not JArray array) return;
            try
            {
                var set = new HashSet<T>();
                foreach (var element in array)
                {
                    if (element.Type == JTokenType.String &&
                        Enum.TryParse(element.ToString(), ignoreCase: true, out T parsed))
                        set.Add(parsed);
                }
                Set(set);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to deserialize multi choice '{GetPath()}': {ex.Message}");
            }
        }
    }
}
