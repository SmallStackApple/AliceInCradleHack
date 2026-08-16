using AliceInCradleHack.events;

namespace AliceInCradleHack.module.modules.combat
{
    public class ModuleVelocity : Module
    {
        public ModuleVelocity() : base("Velocity", "Remove knockback.", "Combat")
        {
        }

        public override void Initialize()
        {
        }

        public override void Enable()
        {
            DamageEvents.Knockback.EventPreKnockback += OnPreKnockback;
        }

        public override void Disable()
        {
            DamageEvents.Knockback.EventPreKnockback -= OnPreKnockback;
        }

        private static void OnPreKnockback(object sender, DamageEvents.Knockback.PreKnockbackEventArgs eventArgs)
        {
            eventArgs.Cancel = true;
        }
    }
}
