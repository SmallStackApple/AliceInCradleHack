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
            harmony.Patch(
                AccessTools.Method(typeof(M2Attackable), nameof(M2Attackable.addKnockbackVelocity)),
                prefix: new HarmonyMethod(typeof(ModuleVelocity), nameof(AddKnockbackVelocityPrefix))
            );
        }

        public override void Disable()
        {
            harmony.UnpatchAll(harmony.Id);
        }

        public override void Initialize()
        {
        }

        public static bool AddKnockbackVelocityPrefix(object __instance)
        {
            return false;
        }
    }
}
