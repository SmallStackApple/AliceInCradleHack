using AliceInCradleHack.config;
using HarmonyLib;
using m2d;
using nel;
using System.Reflection;

namespace AliceInCradleHack.module.modules.combat
{
    /// <summary>
    /// Keeps the player moving while performing light attacks (ground punch / air punch)
    /// instead of being forced to a stop by the attack state.
    /// </summary>
    public class ModuleKeepSprint : Module
    {
        public ModuleKeepSprint() : base("KeepSprint", "Keep moving while light attacking (punch).", "Combat")
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

        private readonly Harmony harmony = new("aliceincradlehack.modules.combat.keepsprint");

        private static readonly MethodInfo RefineMoveKeyMethod = AccessTools.Method(typeof(M2MoverPr), "refineMoveKey", new[] { typeof(bool) });
        private static readonly MethodInfo RunDashPunchMethod = AccessTools.Method(typeof(M2PrSkill), "runDashPunch");
        private static readonly MethodInfo CalcWalkSpeedMethod = AccessTools.Method(typeof(PR), "calcWalkSpeed", new[] { typeof(int) });
        private static readonly AccessTools.FieldRef<M2Mover, M2Phys> PhyAccessor = AccessTools.FieldRefAccess<M2Mover, M2Phys>("Phy");

        private static ModuleKeepSprint _instance;

        public override void Initialize()
        {
        }

        public override void Enable()
        {
            _instance = this;
            if (RefineMoveKeyMethod != null)
            {
                harmony.Patch(
                    RefineMoveKeyMethod,
                    postfix: new HarmonyMethod(typeof(ModuleKeepSprint), nameof(RefineMoveKeyPostfix))
                );
            }
            if (RunDashPunchMethod != null)
            {
                harmony.Patch(
                    RunDashPunchMethod,
                    postfix: new HarmonyMethod(typeof(ModuleKeepSprint), nameof(DashPunchPostfix))
                );
            }
        }

        public override void Disable()
        {
            _instance = null;
            harmony.UnpatchAll(harmony.Id);
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

            if (!isLightAttack) return;

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
    }
}
