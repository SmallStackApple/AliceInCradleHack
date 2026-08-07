using HarmonyLib;

namespace AliceInCradleHack.patch
{
    /// <summary>
    /// Base class for Harmony patches. Owns a Harmony instance whose id is the subclass type name.
    /// </summary>
    public abstract class Patch
    {
        protected Harmony Harmony { get; }

        protected Patch()
        {
            Harmony = new Harmony(GetType().FullName.ToLowerInvariant());
        }

        /// <summary>
        /// Invoked when the hack is initializing.
        /// </summary>
        public abstract void Apply();

        /// <summary>
        /// Invoked when the hack is being removed or disabled.
        /// </summary>
        public abstract void Remove();
    }
}
