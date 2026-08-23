using AliceInCradleHack.utils.game;
using HarmonyLib;
using nel;
using System.Reflection;

namespace AliceInCradleHack.module.modules.combat
{
    /// <summary>
    /// Keeps the native shield guard flow while preventing the shield from breaking.
    /// </summary>
    public sealed class ModuleInfiniteShield : Module
    {
        public ModuleInfiniteShield() : base("InfiniteShield", "The player's shield never breaks.", "Combat")
        {
        }

        private readonly Harmony _harmony = new("aliceincradlehack.modules.combat.infiniteshield");
        private static ModuleInfiniteShield _instance;

        private static readonly MethodInfo CheckShieldMethod = AccessTools.Method(
            typeof(M2Shield), "checkShield", new[] { typeof(m2d.AttackInfo), typeof(float), typeof(bool) });
        private static readonly AccessTools.FieldRef<M2Shield, float> AppearableTimeAccessor =
            AccessTools.FieldRefAccess<M2Shield, float>("appearable_time_");

        private static bool _guardPatched;
        private static float _appearableTime;

        public override void Enable()
        {
            _instance = this;
            if (CheckShieldMethod != null)
            {
                _harmony.Patch(
                    CheckShieldMethod,
                    prefix: new HarmonyMethod(typeof(ModuleInfiniteShield), nameof(CheckShieldPrefix)),
                    postfix: new HarmonyMethod(typeof(ModuleInfiniteShield), nameof(CheckShieldPostfix)));
            }
        }

        public override void Disable()
        {
            _instance = null;
            _guardPatched = false;
            _harmony.UnpatchAll(_harmony.Id);
        }

        private static void CheckShieldPrefix(M2Shield __instance, m2d.AttackInfo Atk, float val, bool from_away)
        {
            _guardPatched = false;
            if (_instance == null || __instance == null || Atk == null || val < 0f) return;
            if (!IsLocalShield(__instance) || !__instance.canGuard()) return;

            // Let the game produce its normal guard effects, but make this hit unable
            // to reach breakShield. The value is restored immediately after the call.
            _appearableTime = AppearableTimeAccessor(__instance);
            AppearableTimeAccessor(__instance) = float.MaxValue;
            _guardPatched = true;
        }

        private static void CheckShieldPostfix(M2Shield __instance)
        {
            if (!_guardPatched || __instance == null) return;
            AppearableTimeAccessor(__instance) = _appearableTime;
            __instance.cure();
            _guardPatched = false;
        }

        private static bool IsLocalShield(M2Shield shield)
        {
            PRNoel player = AliceInCradleHack.utils.game.NelM2DBase.PlayerNoel;
            return player?.Skill?.ShE?.Shield != null && ReferenceEquals(player.Skill.ShE.Shield, shield);
        }
    }
}
