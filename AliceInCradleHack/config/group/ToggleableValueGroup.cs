using System;

namespace AliceInCradleHack.config.group
{
    /// <summary>
    /// A group that can be toggled on and off. Toggling propagates to nested
    /// <see cref="ToggleableValueGroup"/> children.
    /// </summary>
    public class ToggleableValueGroup : ValueGroup
    {
        public readonly Value<bool> EnabledValue;

        public override ValueType Type => ValueType.ToggleableGroup;

        public bool Enabled
        {
            get => EnabledValue.Get();
            set => EnabledValue.Set(value);
        }

        public ToggleableValueGroup(string name, bool enabled = false, string description = null)
            : base(name, description)
        {
            EnabledValue = new Value<bool>("Enabled", enabled, "Whether this group is enabled", ValueType.Boolean);
            Add(EnabledValue);
            EnabledValue.OnChanged(OnToggled);
        }

        /// <summary>
        /// Called after the enabled state changed. Override to react to toggles.
        /// </summary>
        protected virtual void OnToggled(bool state)
        {
            UpdateChildToggleState(state);
        }

        internal void PropagateToggled(bool state) => OnToggled(state);
    }
}
