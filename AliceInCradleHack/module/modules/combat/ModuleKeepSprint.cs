using AliceInCradleHack.config;
using HarmonyLib;
using m2d;
using nel;
using System.Reflection;

namespace AliceInCradleHack.module.modules.combat
{
    /// <summary>
    /// Keeps the player moving while performing light attacks (ground punch / air punch)
    /// or casting the magic shotgun instead of being forced to a stop by the attack state.
    /// </summary>
    public class ModuleKeepSprint : Module
    {
        public ModuleKeepSprint() : base("KeepSprint", "Keep moving while light attacking (punch) or casting magic shotgun.", "Combat")
        {
        }

        /// <summary>
        /// Also apply the effect to the air punch (jump slash).
        /// </summary>
        public readonly Value<bool> IncludeAirPunch = new(true, "Apply to air punch (jump slash) as well.");

        /// <summary>
        /// Also apply the effect to the dash punch (input-driven movement instead of the fixed lunge).
        /// </summary>
        public readonly Value<bool> IncludeDashPunch = new(true, "Apply to dash punch as well (input-driven).");

        /// <summary>
        /// Also apply the effect to the magic shotgun (magic explode prepare / exploded states).
        /// </summary>
        public readonly Value<bool> IncludeMagicShotgun = new(true, "Apply to magic shotgun (magic explode) as well.");

        /// <summary>
        /// Keep movement input during shield attacks, without changing ordinary shield movement.
        /// </summary>
        public readonly Value<bool> IncludeShieldAttacks = new(true, "Apply to shield bush and shield lariat attacks.");

        private readonly Harmony _harmony = new("aliceincradlehack.modules.combat.keepsprint");

        private static readonly MethodInfo RefineMoveKeyMethod = AccessTools.Method(typeof(M2MoverPr), "refineMoveKey", new[] { typeof(bool) });
        private static readonly MethodInfo RunDashPunchMethod = AccessTools.Method(typeof(M2PrSkill), "runDashPunch");
        private static readonly MethodInfo RunEvadeInputtingMethod = AccessTools.Method(
            typeof(M2PrSkillShieldEvade), "runEvadeInputting",
            new[]
            {
                typeof(float), AccessTools.Inner(typeof(M2MoverPr), "DECL"), typeof(float), typeof(float),
                typeof(bool), typeof(float), typeof(bool).MakeByRefType(), typeof(bool).MakeByRefType(), typeof(bool)
            });
        private static readonly MethodInfo RunEvadeInputtingWithVelocityMethod = AccessTools.Method(
            typeof(M2PrSkillShieldEvade), "runEvadeInputting",
            new[]
            {
                typeof(float), AccessTools.Inner(typeof(M2MoverPr), "DECL"), typeof(float), typeof(float),
                typeof(bool), typeof(float), typeof(bool).MakeByRefType(), typeof(bool).MakeByRefType(),
                typeof(float).MakeByRefType(), typeof(bool)
            });
        private static readonly MethodInfo ChangeStateMethod = AccessTools.Method(
            typeof(PR),
            "changeState",
            new[] { AccessTools.Inner(typeof(PR), "STATE"), AccessTools.Inner(typeof(PR), "STATE") }
        );
        private static readonly MethodInfo CalcWalkSpeedMethod = AccessTools.Method(typeof(PR), "calcWalkSpeed", new[] { typeof(int) });
        private static readonly AccessTools.FieldRef<M2Mover, M2Phys> PhyAccessor = AccessTools.FieldRefAccess<M2Mover, M2Phys>("Phy");
        private static readonly FieldInfo StateField = AccessTools.Field(typeof(PR), "state");

        private static ModuleKeepSprint _instance;

        public override void Initialize()
        {
        }

        public override void Enable()
        {
            _instance = this;
            if (RefineMoveKeyMethod != null)
            {
                _harmony.Patch(
                    RefineMoveKeyMethod,
                    postfix: new HarmonyMethod(typeof(ModuleKeepSprint), nameof(RefineMoveKeyPostfix))
                );
            }
            if (RunDashPunchMethod != null)
            {
                _harmony.Patch(
                    RunDashPunchMethod,
                    postfix: new HarmonyMethod(typeof(ModuleKeepSprint), nameof(DashPunchPostfix))
                );
            }
            if (RunEvadeInputtingMethod != null)
            {
                _harmony.Patch(
                    RunEvadeInputtingMethod,
                    prefix: new HarmonyMethod(typeof(ModuleKeepSprint), nameof(RunEvadeInputtingPrefix))
                );
            }
            if (RunEvadeInputtingWithVelocityMethod != null)
            {
                _harmony.Patch(
                    RunEvadeInputtingWithVelocityMethod,
                    prefix: new HarmonyMethod(typeof(ModuleKeepSprint), nameof(RunEvadeInputtingPrefix))
                );
            }
            if (ChangeStateMethod != null)
            {
                _harmony.Patch(
                    ChangeStateMethod,
                    prefix: new HarmonyMethod(typeof(ModuleKeepSprint), nameof(ChangeStatePrefix)),
                    postfix: new HarmonyMethod(typeof(ModuleKeepSprint), nameof(ChangeStatePostfix))
                );
            }
        }

        public override void Disable()
        {
            _instance = null;
            _harmony.UnpatchAll(_harmony.Id);
        }

        private static void RefineMoveKeyPostfix(M2MoverPr __instance)
        {
            if (_instance == null) return;
            if (__instance is not PR pr) return;
            if (!pr.is_alive) return;
            if (pr.isMoveScriptActive(false)) return;

            bool isLightAttack = pr.isPunchState()
                && (!pr.isSpecialPunchState()
                    || (_instance.IncludeAirPunch.Get() && pr.isAirPunchState()));

            bool isMagicShotgun = _instance.IncludeMagicShotgun.Get() && pr.isMagicState();

            if (!isLightAttack && !isMagicShotgun) return;

            int dir = pr.isRO(0, false) ? 1 : pr.isLO(0, false) ? -1 : 0;
            float speed = (float)CalcWalkSpeedMethod.Invoke(pr, new object[] { dir });
            PhyAccessor(pr).walk_xspeed = speed;
        }

        private static void DashPunchPostfix(M2PrSkill __instance)
        {
            if (_instance == null || !_instance.IncludeDashPunch.Get()) return;
            PR pr = __instance.Pr;
            if (pr == null || !pr.is_alive) return;
            if (pr.isMoveScriptActive(false)) return;

            int dir = pr.isRO(0, false) ? 1 : pr.isLO(0, false) ? -1 : 0;
            float speed = (float)CalcWalkSpeedMethod.Invoke(pr, new object[] { dir });
            PhyAccessor(pr).walk_xspeed = speed;
        }

        private static bool IsShieldAttackState(PR pr)
        {
            if (pr == null || StateField == null) return false;
            object state = StateField.GetValue(pr);
            string name = state?.ToString();
            return name == "SHIELD_BUSH" || name == "SHIELD_LARIAT";
        }

        private static void RunEvadeInputtingPrefix(M2PrSkillShieldEvade __instance, ref bool use_walkspeed)
        {
            if (_instance == null || !_instance.IncludeShieldAttacks.Get()) return;
            if (__instance?.Pr == null || !__instance.Pr.is_alive) return;
            if (IsShieldAttackState(__instance.Pr))
                use_walkspeed = true;
        }

        private static void ChangeStatePrefix(PR __instance, object[] __args, out bool __state)
        {
            __state = false;
            if (_instance == null || !_instance.IncludeMagicShotgun.Get()) return;
            if (__args == null || __args.Length != 2) return;
            if (__args[0]?.ToString() != "EVADE_SHOTGUN") return;
            if (__args[1]?.ToString() != "PUNCH") return;

            // A shotgun hit normally forces a long backwards evade. Keep the player in normal movement instead.
            __args[0] = System.Enum.Parse(__args[0].GetType(), "NORMAL");
            __state = true;
        }

        private static void ChangeStatePostfix(PR __instance, bool __state)
        {
            if (!__state) return;

            int dir = __instance.isRO(0, false) ? 1 : __instance.isLO(0, false) ? -1 : 0;
            float speed = (float)CalcWalkSpeedMethod.Invoke(__instance, new object[] { dir });
            PhyAccessor(__instance).walk_xspeed = speed;
        }
    }
}
