using HarmonyLib;
using nel;
using static AliceInCradleHack.Utils.SceneGame;

namespace AliceInCradleHack.Patches
{
    public class PatchNelSceneGame : Patch
    {
        Harmony harmony = new Harmony("aliceincradlehack.patches.patchscenegame");
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
            var playerValue = fieldInfoPlayer.GetValue(__instance) as PRNoel;
            if (PlayerInstance != playerValue && playerValue != null)
            {
                PlayerInstance = playerValue;
            }
        }
    }
}