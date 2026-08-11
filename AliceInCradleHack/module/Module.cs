using AliceInCradleHack.config;
using AliceInCradleHack.config.group;
using System.Reflection;

namespace AliceInCradleHack.module
{
    /// <summary>
    /// Base class for all hack modules. The enabled state is managed centrally by ModuleManager.
    /// Settings are declared as config value fields (see <see cref="Value{T}"/>); they are
    /// automatically registered into <see cref="Settings"/> when the module is registered.
    /// </summary>
    public abstract class Module
    {
        protected Module(string name, string description, string category)
        {
            Name = name;
            Description = description;
            Category = category;
        }

        public string Name { get; }
        public string Description { get; }
        public string Category { get; }
        public virtual bool IsEnabled { get; set; }

        /// <summary>
        /// The root config of this module. Created by ModuleManager during registration.
        /// </summary>
        public Config Settings { get; internal set; }

        /// <summary>
        /// The hidden value persisting this module's enabled state. Managed by ModuleManager.
        /// </summary>
        internal Value<bool> EnabledValue { get; set; }

        public virtual void Initialize() { }
        public virtual void Enable() { }
        public virtual void Disable() { }

        /// <summary>
        /// Scans instance fields for config values and attaches them to <see cref="Settings"/>.
        /// Root-level fields are named after the field (lowerCamelCase) unless overridden by
        /// <see cref="SettingNameAttribute"/>; <see cref="SettingGroupAttribute"/> nests a
        /// field inside a sub group.
        /// </summary>
        internal void AutoRegisterSettings()
        {
            var fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                if (!typeof(Value).IsAssignableFrom(field.FieldType)) continue;
                if (field.GetValue(this) is not Value value) continue;
                if (value.Parent != null) continue;

                var nameAttr = field.GetCustomAttribute<SettingNameAttribute>();
                if (nameAttr != null)
                    value.Name = nameAttr.Name;
                else if (System.String.IsNullOrEmpty(value.Name))
                    value.Name = field.Name;

                var descAttr = field.GetCustomAttribute<SettingDescriptionAttribute>();
                if (descAttr != null)
                    value.Description = descAttr.Description;

                var aliasAttr = field.GetCustomAttribute<SettingAliasAttribute>();
                if (aliasAttr != null)
                    value.Aliases.AddRange(aliasAttr.Aliases);

                var groupAttr = field.GetCustomAttribute<SettingGroupAttribute>();
                if (groupAttr == null)
                    Settings.Add(value);
                else
                    GetOrCreateGroup(groupAttr.GroupName, groupAttr.Description).Add(value);
            }
        }

        private ValueGroup GetOrCreateGroup(string name, string description)
        {
            foreach (var child in Settings.Children)
                if (child is ValueGroup g && g.MatchesName(name)) return g;
            var created = new ValueGroup(name, description);
            Settings.Add(created);
            return created;
        }
    }
}
