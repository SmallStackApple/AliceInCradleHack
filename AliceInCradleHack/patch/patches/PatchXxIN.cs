using AliceInCradleHack.events;
using HarmonyLib;

namespace AliceInCradleHack.patch.patches
{
    public class PatchXxIN : Patch
    {
        public override void Apply()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(XX.IN), "Update"),
                prefix: new HarmonyMethod(typeof(PatchXxIN), nameof(UpdatePrefix)),
                postfix: new HarmonyMethod(typeof(PatchXxIN), nameof(UpdatePostfix))
            );
        }

        public override void Remove()
        {
            _harmony.UnpatchAll(_harmony.Id);
        }

        private static void UpdatePrefix(object __instance)
        {
            XxINEvents.PreUpdate(__instance);
        }

        private static void UpdatePostfix(object __instance)
        {
            XxINEvents.PostUpdate(__instance);
        }
    }
}
