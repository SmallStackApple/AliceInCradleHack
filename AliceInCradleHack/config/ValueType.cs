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
        Color,
        EnumChoice,
        MultiChoice,
        List,
        Group,
        ToggleableGroup,
        ModeGroup,
        Invalid
    }
}
