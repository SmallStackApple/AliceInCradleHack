using AliceInCradleHack.events;
using HarmonyLib;
using m2d;

namespace AliceInCradleHack.patch.patches
{
    /// <summary>
    /// Hooks M2Attackable.applyHpDamage to fire the damage events.
    /// </summary>
    public class PatchM2dM2Attackable : Patch
    {
        public override void Apply()
        {
            Harmony.PatchAll(typeof(M2dM2AttackablePatch).Assembly);
        }

        public override void Remove()
        {
            Harmony.UnpatchAll(Harmony.Id);
        }

        private static class M2dM2AttackablePatch
        {
            [HarmonyPatch(typeof(M2Attackable), "applyHpDamage")]
            private static class ApplyHpDamage
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
