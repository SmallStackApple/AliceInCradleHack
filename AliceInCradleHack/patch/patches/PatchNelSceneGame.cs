using AliceInCradleHack.events;
using AliceInCradleHack.utils.client;
using HarmonyLib;

namespace AliceInCradleHack.patch.patches
{
    /// <summary>
    /// Hooks SceneGame.runIRD to raise <see cref="SceneGameEvents"/> lifecycle events.
    /// </summary>
    public class PatchNelSceneGame : Patch
    {
        public override void Apply()
        {
            var original = AccessTools.Method(typeof(nel.SceneGame), "runIRD");
            if (original == null)
            {
                Log.Error("PatchNelSceneGame: nel.SceneGame.runIRD was not found.");
                return;
            }

            _harmony.Patch(
                original: original,
                prefix: new HarmonyMethod(typeof(PatchNelSceneGame), nameof(RunIrdPrefix)),
                postfix: new HarmonyMethod(typeof(PatchNelSceneGame), nameof(RunIrdPostfix))
            );
        }

        public override void Remove()
        {
            _harmony.UnpatchAll(_harmony.Id);
        }

        private static void RunIrdPrefix(nel.SceneGame __instance, float fcnt)
        {
            if (__instance == null) return;
            SceneGameEvents.PreRunIrd(__instance, fcnt);
        }

        private static void RunIrdPostfix(nel.SceneGame __instance, float fcnt, bool __result)
        {
            if (__instance == null) return;
            SceneGameEvents.PostRunIrd(__instance, fcnt, __result);
        }
    }
}
