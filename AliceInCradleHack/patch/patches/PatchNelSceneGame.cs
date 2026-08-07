using HarmonyLib;
using nel;
using static AliceInCradleHack.utils.game.SceneGame;

namespace AliceInCradleHack.patch.patches
{
    /// <summary>
    /// Hooks SceneGame.runIRD to keep the static game object accessors up to date.
    /// </summary>
    public class PatchNelSceneGame : Patch
    {
        public override void Apply()
        {
            Harmony.Patch(
                original: AccessTools.Method(typeof(SceneGame), "runIRD"),
                prefix: new HarmonyMethod(typeof(PatchNelSceneGame), nameof(RunIrdPrefix))
            );
        }

        public override void Remove()
        {
            Harmony.UnpatchAll(Harmony.Id);
        }

        private static void RunIrdPrefix(object __instance)
        {
            if (__instance == null) return;

            Instance = __instance as SceneGame;

            var playerValue = FieldInfoPlayer.GetValue(__instance) as PRNoel;
            if (PrNoelInstance != playerValue && playerValue != null)
            {
                PrNoelInstance = playerValue;
            }

            var m2dInstance = FieldInfoM2D.GetValue(__instance) as NelM2DBase;
            if (M2DInstance != m2dInstance && m2dInstance != null)
            {
                M2DInstance = m2dInstance;
            }
        }
    }
}
