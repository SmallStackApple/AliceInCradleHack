using System;

namespace AliceInCradleHack.module
{
    /// <summary>
    /// Overrides the auto-generated setting name of a config value field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class SettingNameAttribute : Attribute
    {
        public string Name { get; }
        public SettingNameAttribute(string name) => Name = name;
    }

    /// <summary>
    /// Sets the description of a config value field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class SettingDescriptionAttribute : Attribute
    {
        public string Description { get; }
        public SettingDescriptionAttribute(string description) => Description = description;
    }

    /// <summary>
    /// Adds backwards-compatible aliases for a config value field,
    /// used when deserializing configs written before a rename.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class SettingAliasAttribute : Attribute
    {
        public string[] Aliases { get; }
        public SettingAliasAttribute(params string[] aliases) => Aliases = aliases;
    }

    /// <summary>
    /// Nests a config value field inside a sub group of the module's settings.
    /// Fields sharing the same group name end up in the same group.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class SettingGroupAttribute : Attribute
    {
        public string GroupName { get; }
        public string Description { get; }
        public SettingGroupAttribute(string groupName, string description = null)
        {
            GroupName = groupName;
            Description = description;
        }
    }
}
