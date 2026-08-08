namespace AliceInCradleHack.config
{
    /// <summary>
    /// Metadata describing the kind of a <see cref="Value"/>.
    /// </summary>
    public enum ValueType
    {
        Boolean,
        Int,
        Float,
        Double,
        Text,
        EnumChoice,
        MultiChoice,
        Group,
        ToggleableGroup,
        ModeGroup,
        Invalid
    }
}
