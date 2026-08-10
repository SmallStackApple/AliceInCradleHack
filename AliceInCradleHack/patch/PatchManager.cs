using AliceInCradleHack.patch.patches;
using System;
using System.Collections.Generic;

namespace AliceInCradleHack.patch
{
    /// <summary>
    /// Patch manager (singleton). Registers patches and applies/removes them.
    /// </summary>
    public class PatchManager
    {
        private readonly List<Patch> _patches = new();

        private static readonly Lazy<PatchManager> _lazyInstance = new(() => new PatchManager());
        public static PatchManager Instance => _lazyInstance.Value;

        public IReadOnlyList<Patch> Patches => _patches;

        private PatchManager() { }

        public void Initialize()
        {
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
                    utils.client.Log.Error($"Failed to apply patch {patch.GetType().Name}", ex);
                }
            }
        }

        public void RemoveAllPatches()
        {
            foreach (var patch in _patches)
            {
                patch.Remove();
            }
        }
    }
}
