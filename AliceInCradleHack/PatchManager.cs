using AliceInCradleHack.Patches;
using System;
using System.Collections.Generic;

namespace AliceInCradleHack
{
    public class PatchManager
    {
        public List<Patch> Patches { get; } = new List<Patch>();


        private static readonly Lazy<PatchManager> __lazyInstance = new Lazy<PatchManager>(() => new PatchManager());
        public static PatchManager Instance { get; } = __lazyInstance.Value;

        public void Initialize()
        {
            List<Patch> buildInPatches = new List<Patch>
            {
                new PatchM2dM2Attackable(),
                new PatchNelSceneGame(),
            };
            foreach (var patch in buildInPatches)
            {
                AddPatch(patch);
            }
            ApplyAllPatches();
        }
        public void AddPatch(Patch patch)
        {
            Patches.Add(patch);
        }
        public void ApplyPatch(Patch patch)
        {
            patch.Apply();
        }
        public void ApplyAllPatches()
        {
            foreach (var patch in Patches)
            {
                patch.Apply();
            }
        }
        public void RemoveAllPatches()
        {
            foreach (var patch in Patches)
            {
                patch.Remove();
            }
        }
    }
}
