using System;
using System.Globalization;

namespace AliceInCradleHack.config
{
    /// <summary>
    /// Non-generic view of a <see cref="RangedValue{T}"/>, for tools (WebUI) that cannot
    /// easily use the generic type.
    /// </summary>
    public interface IRangedValue
    {
        object MinObject { get; }
        object MaxObject { get; }
        string Suffix { get; }
    }

    /// <summary>
    /// A numeric value constrained to a [Min, Max] range. Incoming values are clamped.
    /// </summary>
    public class RangedValue<T> : Value<T>, IRangedValue where T : IComparable<T>
    {
        public T Min { get; set; }
        public T Max { get; set; }
        public string Suffix { get; set; } = "";

        object IRangedValue.MinObject => Min;
        object IRangedValue.MaxObject => Max;
        string IRangedValue.Suffix => Suffix;

        public RangedValue(T defaultValue, string description = null)
            : base(defaultValue, description, GuessType()) { }

        public RangedValue(string name, T defaultValue, string description = null)
            : base(name, defaultValue, description, GuessType()) { }

        public RangedValue(T defaultValue, T min, T max, string suffix = "", string description = null)
            : this(defaultValue, description)
        {
            Min = min;
            Max = max;
            Suffix = suffix;
        }

        public RangedValue(string name, T defaultValue, T min, T max, string suffix = "", string description = null)
            : this(name, defaultValue, description)
        {
            Min = min;
            Max = max;
            Suffix = suffix;
        }

        public override void Set(T value)
        {
            base.Set(Clamp(value));
        }

        protected T Clamp(T value)
        {
            if (Min != null && value.CompareTo(Min) < 0) return Min;
            if (Max != null && value.CompareTo(Max) > 0) return Max;
            return value;
        }

        protected override T Parse(string value)
        {
            var t = typeof(T);
            if (t == typeof(int)) return (T)(object)int.Parse(value, CultureInfo.InvariantCulture);
            if (t == typeof(float)) return (T)(object)float.Parse(value, CultureInfo.InvariantCulture);
            if (t == typeof(double)) return (T)(object)double.Parse(value, CultureInfo.InvariantCulture);
            if (t == typeof(long)) return (T)(object)long.Parse(value, CultureInfo.InvariantCulture);
            return base.Parse(value);
        }

        public override string ToString() => $"RangedValue(name={Name}, value={_inner}, range=[{Min}..{Max}]{Suffix})";
    }
}
