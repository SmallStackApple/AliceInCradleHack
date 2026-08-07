using System;

namespace AliceInCradleHack.module.settings
{
    /// <summary>
    /// Fluent builder for a module's settings tree.
    /// </summary>
    public class SettingBuilder
    {
        private readonly SettingGroup _root;
        private SettingGroup _current;

        public SettingBuilder(string name = "Root", string description = null)
        {
            _root = new SettingGroup(name, description);
            _current = _root;
        }

        public SettingBuilder Group(string name, string description = null)
        {
            _current = _current.Children.TryGetValue(name, out var existing)
                ? (SettingGroup)existing
                : AddChild(new SettingGroup(name, description));
            return this;
        }

        public SettingBuilder Add<T>(string name, string description, T defaultValue, bool isEditable = true)
        {
            _current.AddChild(new Setting<T>(name, description, defaultValue, isEditable));
            return this;
        }

        public SettingBuilder Back()
        {
            if (_current.Parent is SettingGroup p) _current = p;
            return this;
        }

        public SettingBuilder Reset()
        {
            _current = _root;
            return this;
        }

        public SettingGroup Build() => _root;

        public static SettingGroup Create(Action<SettingBuilder> buildAction)
        {
            var b = new SettingBuilder();
            buildAction(b);
            return b.Build();
        }

        private SettingGroup AddChild(SettingGroup child)
        {
            _current.AddChild(child);
            return child;
        }
    }
}
