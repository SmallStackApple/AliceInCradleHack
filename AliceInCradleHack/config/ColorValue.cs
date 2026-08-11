using System.Text.RegularExpressions;

namespace AliceInCradleHack.config
{
    /// <summary>
    /// A color value stored as a hex string like "#RRGGBB" or "#RRGGBBAA".
    /// Invalid values are rejected.
    /// </summary>
    public class ColorValue : Value<string>
    {
        private static readonly Regex HexPattern = new Regex("^#[0-9A-Fa-f]{6}([0-9A-Fa-f]{2})?$", RegexOptions.Compiled);

        public ColorValue(string name, string defaultValue, string description = null)
            : base(name, Normalize(defaultValue), description, ValueType.Color)
        {
        }

        public ColorValue(string defaultValue, string description = null)
            : base(Normalize(defaultValue), description, ValueType.Color)
        {
        }

        private static string Normalize(string value)
        {
            if (value == null) return "#000000";
            var trimmed = value.Trim();
            return HexPattern.IsMatch(trimmed) ? trimmed.ToUpperInvariant() : "#000000";
        }

        protected override string Parse(string value)
        {
            return Normalize(value);
        }

        public override void Set(string value)
        {
            base.Set(Normalize(value));
        }
    }
}
