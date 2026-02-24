using HarmonyLib;
using nel;
using static AliceInCradleHack.Utils.SceneGame;

namespace AliceInCradleHack.Patches
{
    public class PatchNelSceneGame : Patch
    {
        Harmony harmony = new Harmony("aliceincradlehack.patches.patchnelscenegame");
        public override void Apply()
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(SceneGame), "runIRD"),
                prefix: new HarmonyMethod(typeof(PatchNelSceneGame), nameof(runIRDPrefix))
            );
        }

        public override void Remove()
        {
            harmony.UnpatchAll(harmony.Id);
        }

        private static void runIRDPrefix(object __instance)
        {
            if (__instance != null)
            {
                Instance = __instance as SceneGame;
            }

            var playerValue = fieldInfoPlayer.GetValue(__instance) as PRNoel;
            if (PrNoelInstance != playerValue && playerValue != null)
            {
                PrNoelInstance = playerValue;
            }

            var m2dinstance = fieldInfoM2D.GetValue(__instance) as NelM2DBase;
            if (M2DInstance != m2dinstance && m2dinstance != null)
            {
                M2DInstance = m2dinstance;
            }
        }
    }
}