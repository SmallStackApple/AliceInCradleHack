using AliceInCradleHack.events;
using AliceInCradleHack.utils.client;
using HarmonyLib;
using m2d;

namespace AliceInCradleHack.patch.patches
{
    /// <summary>
    /// Hooks M2Attackable to fire the damage and knockback events.
    /// </summary>
    public class PatchM2dM2Attackable : Patch
    {
        public override void Apply()
        {
            var hpDamageReplacements = _harmony.CreateClassProcessor(typeof(ApplyHpDamage)).Patch();
            var knockbackReplacements = _harmony.CreateClassProcessor(typeof(AddKnockbackVelocity)).Patch();
            int count = (hpDamageReplacements?.Count ?? 0) + (knockbackReplacements?.Count ?? 0);
            Log.Info($"Patch {GetType().Name} applied ({count} method(s) patched)");
        }

        public override void Remove()
        {
            _harmony.UnpatchAll(_harmony.Id);
        }

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

        [HarmonyPatch(typeof(M2Attackable), "addKnockbackVelocity")]
        private static class AddKnockbackVelocity
        {
            [HarmonyPrefix]
            public static bool PreAddKnockbackVelocity(object __instance, ref float v0, ref AttackInfo Atk, ref M2Attackable Another)
            {
                return !DamageEvents.Knockback.PreKnockback(__instance, ref v0, ref Atk, ref Another);
            }
        }
    }
}
