using HarmonyLib;
using m2d;

namespace AliceInCradleHack.Modules.Combat
{
    public class ModuleVelocity : Module
    {
        public override string Name => "Velocity";

        public override string Description => "Remove knockback.";

        public override string Author => "SmallStackApple";

        public override string Version => "1.0.0";

        public override bool IsEnabled { get; set; } = false;

        public override SettingNode Settings { get; } = new SettingBuilder()
            .Build();

        private readonly Harmony harmony = new("aliceincradlehack.modules.combat.velocity");

        public override void Disable()
        {
            harmony.UnpatchAll(harmony.Id);
        }

        public override void Enable()
        {
            harmony.PatchAll(typeof(ModuleVelocity).Assembly);
        }

        public override void Initialize(){}

        [HarmonyPatch(typeof(M2Attackable), nameof(M2Attackable.addKnockbackVelocity))]
        [HarmonyPrefix]
        public static bool addKnockbackVelocityPrefix(object __instance)
        {
            return false;
        }
    }
}
