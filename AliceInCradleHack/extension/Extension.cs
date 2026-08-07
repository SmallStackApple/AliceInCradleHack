namespace AliceInCradleHack.extension
{
    /// <summary>
    /// Base class for external extensions loaded from DLLs at runtime.
    /// </summary>
    public abstract class Extension
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public bool IsLoaded { get; internal set; }

        public abstract void Load();
        public abstract void Unload();
    }
}
