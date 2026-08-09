using AliceInCradleHack.utils.client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace AliceInCradleHack.config.group
{
    /// <summary>
    /// A named option of a <see cref="ModeValueGroup{T}"/>, carrying its own child values.
    /// </summary>
    public class Mode : ValueGroup
    {
        public Mode(string name, string description = null) : base(name, description) { }
    }

    /// <summary>
    /// A group that switches between several modes. Each mode has its own settings.
    /// Serialized as { name, active, choices: { modeName: {...} } }.
    /// </summary>
    public class ModeValueGroup<T> : ValueGroup where T : Mode
    {
        private readonly List<T> _modes = new List<T>();
        private int _activeIndex;

        public IReadOnlyList<T> Modes => _modes;

        public override ValueType Type => ValueType.ModeGroup;

        public T ActiveMode => _modes.Count == 0 ? null : _modes[_activeIndex];

        public ModeValueGroup(string name, string description = null) : base(name, description) { }

        public ModeValueGroup(string name, params T[] modes) : this(name, (string)null)
        {
            foreach (var mode in modes) AddMode(mode);
        }

        public T AddMode(T mode)
        {
            if (mode.Parent != null && mode.Parent != this)
                throw new InvalidOperationException($"'{mode.Name}' already belongs to '{mode.Parent.GetPath()}'");
            if (mode.Parent == null)
            {
                mode.Parent = this;
            }
            _modes.Add(mode);
            return mode;
        }

        public void SetActiveByName(string name)
        {
            for (int i = 0; i < _modes.Count; i++)
            {
                if (_modes[i].MatchesName(name))
                {
                    if (_activeIndex != i)
                    {
                        _activeIndex = i;
                        OnActiveModeChanged(_modes[i]);
                    }
                    return;
                }
            }
            throw new ArgumentException($"Mode '{name}' not found in '{GetPath()}'");
        }

        /// <summary>
        /// Called after the active mode changed.
        /// </summary>
        protected virtual void OnActiveModeChanged(T mode) { }

        public override object GetValueObject() => ActiveMode?.Name;

        public override bool SetByString(string value)
        {
            try
            {
                SetActiveByName(value);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to set mode of '{GetPath()}'", ex);
                return false;
            }
        }

        public override JObject ToJToken()
        {
            var choices = new JObject();
            foreach (var mode in _modes)
                choices[mode.Name] = mode.ToJToken();

            var obj = base.ToJToken();
            obj["active"] = ActiveMode?.Name;
            obj["choices"] = choices;
            return obj;
        }

        public override void FromJToken(JObject obj)
        {
            if (obj == null) return;

            string active = obj["active"]?.ToString();
            if (!string.IsNullOrEmpty(active))
            {
                try { SetActiveByName(active); }
                catch (Exception ex) { Log.Error($"Unable to deserialize active mode of '{GetPath()}'", ex); }
            }

            if (obj["choices"] is JObject choices)
            {
                foreach (var mode in _modes)
                {
                    JToken modeToken = choices[mode.Name];
                    if (modeToken == null)
                    {
                        foreach (var alias in mode.Aliases)
                        {
                            modeToken = choices[alias];
                            if (modeToken != null) break;
                        }
                    }
                    if (modeToken is not JObject modeObj) continue;
                    try { mode.FromJToken(modeObj); }
                    catch (Exception ex) { Log.Error($"Unable to deserialize mode '{mode.Name}' of '{GetPath()}'", ex); }
                }
            }

            base.FromJToken(obj);
        }
    }
}
