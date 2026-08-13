using AliceInCradleHack.utils.client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AliceInCradleHack.config
{
    /// <summary>
    /// An editable ordered list of strings, serialized as a JSON array.
    /// </summary>
    public class StringListValue : Value<List<string>>
    {
        public StringListValue(IEnumerable<string> defaultValue = null, string description = null)
            : base(defaultValue == null ? new List<string>() : Normalize(defaultValue), description, ValueType.List) { }

        public StringListValue(string name, IEnumerable<string> defaultValue = null, string description = null)
            : base(name, defaultValue == null ? new List<string>() : Normalize(defaultValue), description, ValueType.List) { }

        public IReadOnlyList<string> Items => _inner;

        public override void Set(List<string> value)
        {
            base.Set(Normalize(value ?? Enumerable.Empty<string>()));
        }

        public override bool SetByString(string value)
        {
            Set(ParseItems(value));
            return true;
        }

        public override bool SetValueObject(object value)
        {
            if (value is string text)
            {
                SetByString(text);
                return true;
            }

            if (value is IEnumerable<string> strings)
            {
                Set(strings.ToList());
                return true;
            }

            if (value is System.Collections.IEnumerable values)
            {
                Set(values.Cast<object>().Select(item => item?.ToString()).ToList());
                return true;
            }

            return base.SetValueObject(value);
        }

        public override JObject ToJToken() => new JObject
        {
            ["name"] = Name,
            ["value"] = new JArray(_inner)
        };

        public override void FromJToken(JObject obj)
        {
            if (obj == null || obj["value"] is not JArray array) return;

            try
            {
                Set(array.Values<string>().ToList());
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to deserialize string list '{GetPath()}'", ex);
            }
        }

        private static List<string> ParseItems(string value)
        {
            return Normalize((value ?? "").Split(new[] { ',', ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
        }

        private static List<string> Normalize(IEnumerable<string> values)
        {
            var result = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var value in values)
            {
                string item = value?.Trim();
                if (!string.IsNullOrEmpty(item) && seen.Add(item))
                    result.Add(item);
            }
            return result;
        }
    }
}
