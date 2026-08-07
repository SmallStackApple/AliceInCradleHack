using AliceInCradleHack.module.settings;
using HarmonyLib;
using m2d;

namespace AliceInCradleHack.module.modules.combat
{
    public class ModuleVelocity : Module
    {
        public override string Name => "Velocity";
        public override string Description => "Remove knockback.";
        public override string Author => "SmallStackApple";
        public override string Version => "1.0.0";

        public override SettingNode Settings { get; } = new SettingBuilder()
            .Build();

        private readonly Harmony _harmony = new("aliceincradlehack.modules.combat.velocity");

        public override void Enable()
        {
            _harmony.PatchAll(typeof(ModuleVelocity).Assembly);
        }

        public override void Disable()
        {
            _harmony.UnpatchAll(_harmony.Id);
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
