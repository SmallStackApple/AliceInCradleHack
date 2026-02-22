using AliceInCradleHack.Events;
using HarmonyLib;
using m2d;

namespace AliceInCradleHack.Patches
{
    public class PatchM2dM2Attackable : Patch
    {
        Harmony harmony = new Harmony("aliceincradlehack.patches.patchm2dm2attackable");

        public override void Apply()
        {
            harmony.PatchAll(typeof(M2dM2AttackablePatch).Assembly);
        }
        public override void Remove()
        {
            harmony.UnpatchAll(harmony.Id);
        }

        private static class M2dM2AttackablePatch
        {
            [HarmonyPatch(typeof(M2Attackable), "applyHpDamage")]
            private static class applyHpDamage
            {
                [HarmonyPrefix]
                public static void PreApplyHpDamage(object __instance, object[] __args)
                {
                    DamageEvents.HpDamage.PreDamage(__instance, __args);
                }

                [HarmonyPostfix]
                public static void PostApplyHpDamage(object __instance, ref int __result, object[] __args)
                {
                    DamageEvents.HpDamage.PostDamage(__instance, ref __result, __args);
                }
            }
        }
    }
}
