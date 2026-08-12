using AliceInCradleHack.patch.patches;
using AliceInCradleHack.utils.client;
using System;
using System.Collections.Generic;

namespace AliceInCradleHack.patch
{
    /// <summary>
    /// Patch manager (singleton). Registers patches and applies/removes them.
    /// </summary>
    public class PatchManager : IClientComponent
    {
        private readonly List<Patch> _patches = new();
        private bool _initialized;

        private static readonly Lazy<PatchManager> _lazyInstance = new(() => new PatchManager());
        public static PatchManager Instance => _lazyInstance.Value;

        public IReadOnlyList<Patch> Patches => _patches;

        private PatchManager() { }

        /// <summary>
        /// Registers and applies the built-in patches. Idempotent; only the first call has an effect.
        /// </summary>
        public void Initialize()
        {
            if (_initialized) return;

            List<Patch> builtInPatches = new()
            {
                new PatchM2dM2Attackable(),
                new PatchXxIN(),
                // Add other patch instances here
            };
            foreach (var patch in builtInPatches)
            {
                AddPatch(patch);
            }
            ApplyAllPatches();
            _initialized = true;
        }

        public void AddPatch(Patch patch)
        {
            _patches.Add(patch);
        }

        public void ApplyPatch(Patch patch)
        {
            patch.Apply();
        }

        public void ApplyAllPatches()
        {
            foreach (var patch in _patches)
            {
                try
                {
                    patch.Apply();
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to apply patch {patch.GetType().Name}", ex);
                }
            }
        }

        /// <summary>
        /// Unregisters a patch (removes its Harmony patches and detaches it from the manager).
        /// </summary>
        public bool RemovePatch(Patch patch)
        {
            if (patch == null || !_patches.Remove(patch)) return false;

            try
            {
                patch.Remove();
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to remove patch {patch.GetType().Name}", ex);
            }
            return true;
        }

        /// <summary>
        /// Removes all patches in reverse registration order. An exception from one patch
        /// does not prevent the remaining ones from being removed.
        /// </summary>
        public void RemoveAllPatches()
        {
            for (int i = _patches.Count - 1; i >= 0; i--)
            {
                try
                {
                    _patches[i].Remove();
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to remove patch {_patches[i].GetType().Name}", ex);
                }
            }
        }

        public void Dispose()
        {
            if (!_initialized && _patches.Count == 0) return;
            RemoveAllPatches();
            _patches.Clear();
            _initialized = false;
        }
    }
}
