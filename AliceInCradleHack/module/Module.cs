using AliceInCradleHack.module.settings;

namespace AliceInCradleHack.module
{
    /// <summary>
    /// Base class for all hack modules. The enabled state is managed centrally by ModuleManager.
    /// </summary>
    public abstract class Module
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string Author { get; }
        public abstract string Version { get; }
        public virtual bool IsEnabled { get; set; }
        public virtual string Category => "General";
        public abstract SettingNode Settings { get; }
        public abstract void Initialize();
        public abstract void Enable();
        public abstract void Disable();
    }
}
