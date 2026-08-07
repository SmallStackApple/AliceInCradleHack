using Newtonsoft.Json.Linq;
using System;

namespace AliceInCradleHack.module.settings
{
    /// <summary>
    /// A leaf settings node holding a typed value.
    /// </summary>
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

        public override object GetValue() => _value;
        public override void SetValue(object value)
            => _value = (T)Convert.ChangeType(value, typeof(T));

        public override JToken ToJToken() => JToken.FromObject(_value);
        public override void FromJToken(JToken token) => _value = token.ToObject<T>();
    }
}
