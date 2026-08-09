using HarmonyLib;
using m2d;

namespace AliceInCradleHack.module.modules.combat
{
    public class ModuleVelocity : Module
    {
        public ModuleVelocity() : base("Velocity", "Remove knockback.", "Combat")
        {
        }

        private readonly Harmony harmony = new("aliceincradlehack.modules.combat.velocity");

        public override void Enable()
        {
            harmony.PatchAll(typeof(ModuleVelocity).Assembly);
        }

        public override void Disable()
        {
            harmony.UnpatchAll(harmony.Id);
        }

        public override void Initialize()
        {
        }

        [HarmonyPatch(typeof(M2Attackable), nameof(M2Attackable.addKnockbackVelocity))]
        [HarmonyPrefix]
        public static bool AddKnockbackVelocityPrefix(object __instance)
        {
            return false;
        }
    }
}
