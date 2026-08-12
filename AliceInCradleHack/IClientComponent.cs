using System;

namespace AliceInCradleHack
{
    /// <summary>
    /// Common lifecycle contract for client components (managers, console, ...).
    /// <see cref="Initialize"/> must be idempotent (repeated calls are no-ops) and
    /// <see cref="Dispose"/> must release every resource the component owns and leave it
    /// safe to be initialized again.
    /// </summary>
    public interface IClientComponent : IDisposable
    {
        void Initialize();
    }
}
