using AliceInCradleHack.events;
using AliceInCradleHack.utils.game;
using HarmonyLib;
using nel;
using System.Reflection;

namespace AliceInCradleHack.module.modules.combat
{
    /// <summary>
    /// Keeps the local player's native shield active without changing movement state.
    /// </summary>
    public sealed class ModuleKeepShield : Module
    {
        public ModuleKeepShield() : base("KeepShield", "Keep the player's shield enabled.", "Combat")
        {
        }

        public override void Enable()
        {
            XxINEvents.EventPostUpdate += OnPostUpdate;
        }

        private static readonly FieldInfo ShieldStateField = AccessTools.Field(typeof(M2Shield), "stt");

        public override void Disable()
        {
            XxINEvents.EventPostUpdate -= OnPostUpdate;
        }

        private static void OnPostUpdate(object sender, XxINEvents.UpdateEventArgs eventArgs)
        {
            PRNoel player = AliceInCradleHack.utils.game.NelM2DBase.PlayerNoel;
            M2Shield shield = player?.Skill?.ShE?.Shield;
            if (player == null || shield == null || !player.is_alive) return;

            if (IsBroken(shield) || shield.alpha <= 0f)
            {
                if (IsBroken(shield))
                {
                    shield.resetValue(false);
                }
                // Keep the shield visible even when native recovery or another state
                // temporarily hides it. Do not patch deactivate(), so native shield
                // attacks can still run their own animation and movement logic.
                shield.activate(true, false);
                shield.cure();
            }
        }

        private static bool IsBroken(M2Shield shield)
        {
            return ShieldStateField?.GetValue(shield)?.ToString() == "BROKEN";
        }
    }
}
