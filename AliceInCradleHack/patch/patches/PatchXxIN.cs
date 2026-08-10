using AliceInCradleHack.module.modules.client.island;
using HarmonyLib;

namespace AliceInCradleHack.patch.patches
{
    public class PatchXxIN : Patch
    {
        public override void Apply()
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(XX.IN), "Update"),
                postfix: new HarmonyMethod(typeof(PatchXxIN), nameof(UpdatePostfix))
            );
        }

        public override void Remove()
        {
            harmony.UnpatchAll(harmony.Id);
        }

        private static void UpdatePostfix()
        {
            GuiBehaviour.EnsureCreated();
        }
    }
}
