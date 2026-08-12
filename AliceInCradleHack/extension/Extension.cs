using System;

namespace AliceInCradleHack.extension
{
    /// <summary>
    /// Base class for external extensions loaded from DLLs at runtime.
    /// <see cref="Initialize"/> is called once after instantiation; <see cref="Dispose"/> is
    /// called when the extension is unloaded or the hack shuts down. An extension must undo
    /// everything it registered in <see cref="Initialize"/> inside <see cref="Dispose"/>.
    /// </summary>
    public abstract class Extension : IDisposable
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public bool IsLoaded { get; internal set; }

        public abstract void Initialize();
        public abstract void Dispose();
    }
}
