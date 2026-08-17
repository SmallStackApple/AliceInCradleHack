using AliceInCradleHack.config;
using AliceInCradleHack.utils.client;
using HarmonyLib;
using nel;
using System;
using System.Reflection;
using XX;

namespace AliceInCradleHack.module.modules.combat
{
    /// <summary>
    /// Rapid attack: speeds up melee attack animations and magic chanting, shortens the
    /// post-attack recovery, and lets light attacks (including magic shotgun punches)
    /// auto-fire at high speed while the attack key is held. While the key is held the
    /// charged magic is never depleted; releasing the key exhausts it automatically.
    /// </summary>
    public class ModuleRapidAttack : Module
    {
        public ModuleRapidAttack() : base("RapidAttack", "Rapid melee / magic chant, hold-to-autofire light attacks and magic shotgun.", "Combat")
        {
        }

        /// <summary>
        /// Speed multiplier applied to melee attack animations and magic chanting.
        /// </summary>
        public readonly RangedValue<double> SpeedMultiplier = new(2.0d, 1.0, 10.0, "x", "Attack / chant speed multiplier.");

        /// <summary>
        /// Apply the speed multiplier to melee attacks (punch / dash punch / air punch / wheel / comet / smash).
        /// </summary>
        public readonly Value<bool> ApplyToMelee = new(true, "Speed up melee attack animations.");

        /// <summary>
        /// Apply the speed multiplier to magic chanting.
        /// </summary>
        public readonly Value<bool> ApplyToMagicChant = new(true, "Speed up magic chanting.");

        /// <summary>
        /// Shorten the post-attack recovery (punch decline time) by the speed multiplier.
        /// </summary>
        public readonly Value<bool> ReducePunchDecline = new(true, "Shorten post-attack recovery by the speed multiplier.");

        /// <summary>
        /// Repeatedly trigger light attacks (and magic shotgun when a charged magic is held) while the attack key is held.
        /// </summary>
        public readonly Value<bool> AutoFire = new(true, "Hold the light attack key to rapidly trigger light attacks / magic shotgun.");

        /// <summary>
        /// Frames between auto-fire triggers while the attack key is held.
        /// </summary>
        public readonly RangedValue<int> AutoFireInterval = new(2, 1, 30, "frames", "Frames between auto-fire triggers.");

        /// <summary>
        /// While the attack key is held, the charged magic is not depleted by shotgun hits.
        /// Releasing the key exhausts the retained charge automatically.
        /// </summary>
        public readonly Value<bool> KeepCharge = new(true, "Charged magic is not depleted while holding the attack key; releasing exhausts it.");

        private readonly Harmony _harmony = new("aliceincradlehack.modules.combat.rapidattack");

        private static ModuleRapidAttack _instance;

        /// <summary>True while a charge retained by KeepCharge is still being held.</summary>
        private static bool _retained;

        /// <summary>Snapshot of the held magic kind before a shotgun hit digests it.</summary>
        private static bool _shotgunSnapshotValid;
        private static MGKIND _shotgunSnapshotKind = MGKIND.NONE;

        /// <summary>IN.totalframe of the last synthetic release.</summary>
        private static int _lastAutoFireFrame = -1;

        private static readonly MethodInfo PunchSpeedMethod = AccessTools.Method(typeof(M2PrSkill), "PunchSpeed");
        private static readonly MethodInfo GetCastingTimeScaleMethod = AccessTools.Method(typeof(PR), "getCastingTimeScale");
        private static readonly MethodInfo SetPunchDeclineTimeMethod = AccessTools.Method(typeof(M2PrSkill), "set_punch_decline_time");
        private static readonly MethodInfo RunPunchCheckMethod = AccessTools.Method(typeof(M2PrSkill), "runPunchCheck");
        private static readonly MethodInfo IsMagicOMethod = AccessTools.Method(typeof(M2PrAssistant), "isMagicO");
        private static readonly MethodInfo DigestShotgunHoldMpMethod = AccessTools.Method(typeof(M2PrSkill), "digestShotgunHoldMp");
        private static readonly MethodInfo RunPreMethod = AccessTools.Method(typeof(PR), nameof(PR.runPre));

        private static readonly AccessTools.FieldRef<M2PrSkill, MagicItem> CurMgAccessor = AccessTools.FieldRefAccess<M2PrSkill, MagicItem>("CurMg");
        private static readonly AccessTools.FieldRef<M2PrSkill, float> MpHoldAccessor = AccessTools.FieldRefAccess<M2PrSkill, float>("mp_hold");
        private static readonly AccessTools.FieldRef<M2PrSkill, float> MpOverholdAccessor = AccessTools.FieldRefAccess<M2PrSkill, float>("mp_overhold");
        private static readonly AccessTools.FieldRef<M2PrSkill, byte> PunchDeclineAccessor = AccessTools.FieldRefAccess<M2PrSkill, byte>("punch_decline_time_");
        private static readonly AccessTools.FieldRef<M2PrSkill, NelPlayerCursor> CursorAccessor = AccessTools.FieldRefAccess<M2PrSkill, NelPlayerCursor>("Cursor");
        private static readonly FieldInfo InputKeyAccessor = AccessTools.Field(typeof(IN), "KA");
        private static readonly FieldInfo InputActionsAccessor = AccessTools.Field(typeof(KEY), "AInputs");
        private static readonly MethodInfo AttackInputIsOnMethod = AccessTools.Method(InputActionsAccessor.FieldType.GetElementType(), "isOn", new[] { typeof(bool) });

        public override void Initialize()
        {
        }

        public override void Enable()
        {
            _instance = this;
            _retained = false;
            _shotgunSnapshotValid = false;
            _lastAutoFireFrame = -1;

            if (PunchSpeedMethod != null)
            {
                _harmony.Patch(
                    PunchSpeedMethod,
                    postfix: new HarmonyMethod(typeof(ModuleRapidAttack), nameof(PunchSpeedPostfix))
                );
            }
            if (GetCastingTimeScaleMethod != null)
            {
                _harmony.Patch(
                    GetCastingTimeScaleMethod,
                    postfix: new HarmonyMethod(typeof(ModuleRapidAttack), nameof(GetCastingTimeScalePostfix))
                );
            }
            if (SetPunchDeclineTimeMethod != null)
            {
                _harmony.Patch(
                    SetPunchDeclineTimeMethod,
                    prefix: new HarmonyMethod(typeof(ModuleRapidAttack), nameof(SetPunchDeclineTimePrefix))
                );
            }
            if (RunPunchCheckMethod != null)
            {
                _harmony.Patch(
                    RunPunchCheckMethod,
                    prefix: new HarmonyMethod(typeof(ModuleRapidAttack), nameof(RunPunchCheckPrefix))
                );
            }
            if (DigestShotgunHoldMpMethod != null)
            {
                _harmony.Patch(
                    DigestShotgunHoldMpMethod,
                    prefix: new HarmonyMethod(typeof(ModuleRapidAttack), nameof(DigestShotgunPrefix)),
                    postfix: new HarmonyMethod(typeof(ModuleRapidAttack), nameof(DigestShotgunPostfix))
                );
            }
            if (RunPreMethod != null)
            {
                _harmony.Patch(
                    RunPreMethod,
                    prefix: new HarmonyMethod(typeof(ModuleRapidAttack), nameof(RunPrePrefix))
                );
            }
        }

        public override void Disable()
        {
            _instance = null;
            _retained = false;
            _shotgunSnapshotValid = false;
            _harmony.UnpatchAll(_harmony.Id);
        }

        private static void PunchSpeedPostfix(ref float __result)
        {
            ModuleRapidAttack module = _instance;
            if (module == null || !module.ApplyToMelee.Get()) return;
            __result *= (float)module.SpeedMultiplier.Get();
        }

        private static void GetCastingTimeScalePostfix(ref float __result)
        {
            ModuleRapidAttack module = _instance;
            if (module == null || !module.ApplyToMagicChant.Get()) return;
            __result *= (float)module.SpeedMultiplier.Get();
        }

        private static void SetPunchDeclineTimePrefix(ref int value)
        {
            ModuleRapidAttack module = _instance;
            if (module == null || !module.ReducePunchDecline.Get()) return;
            value = (int)(value / module.SpeedMultiplier.Get());
        }

        /// <summary>
        /// Converts an auto-fire interval into a single native attack release. The game
        /// itself still resolves the punch variation, changes state, and creates its ray.
        /// </summary>
        private static void RunPunchCheckPrefix(M2PrSkill __instance, float TS)
        {
            ModuleRapidAttack module = _instance;
            if (module == null || !module.AutoFire.Get()) return;
            if (TS <= 0f) return;
            if (!IsLocalPlayerSkill(__instance)) return;

            PR pr = __instance.Pr;
            if (pr == null) return;

            if (!pr.is_alive || pr.isMoveScriptActive(false)) return;
            // Town warp and Burst selection use punch_t as a long-press counter. Exclude
            // only their magic-key input so ordinary non-combat light attacks still fire.
            if (pr.isBurstAllocState() && IsMagicHeld(__instance)) return;
            if (!IsAttackHeld(pr)) return;
            if (!pr.isNormalState()) return;
            if (__instance.magic_t != 0f) return;
            if (PunchDeclineAccessor(__instance) > 0) return;
            if (IN.totalframe - _lastAutoFireFrame < module.AutoFireInterval.Get()) return;

            // A negative timer enters the native release branch on subsequent held frames.
            // Do not override isAtkPD/isAtkO: the initial press and combinations must keep
            // their real input state so getPunchVariation can select the requested skill.
            __instance.punch_t = -0.0001f;
            _lastAutoFireFrame = IN.totalframe;
        }

        private static void DigestShotgunPrefix(M2PrSkill __instance)
        {
            ModuleRapidAttack module = _instance;
            _shotgunSnapshotValid = false;
            if (module == null || !module.KeepCharge.Get()) return;
            if (!IsLocalPlayerSkill(__instance)) return;

            MagicItem curMg = CurMgAccessor(__instance);
            if (curMg == null || curMg.killed) return;

            _shotgunSnapshotKind = curMg.kind;
            _shotgunSnapshotValid = true;
        }

        private static void DigestShotgunPostfix(M2PrSkill __instance)
        {
            ModuleRapidAttack module = _instance;
            if (!_shotgunSnapshotValid) return;
            _shotgunSnapshotValid = false;
            if (module == null || !module.KeepCharge.Get()) return;
            if (!IsLocalPlayerSkill(__instance)) return;

            PR pr = __instance.Pr;
            // Attack key released: let the charge deplete (and the exhaust path finishes it).
            if (pr == null || !IsAttackHeld(pr)) return;

            try
            {
                MagicItem curMg = CurMgAccessor(__instance);
                if (curMg != null && !curMg.killed)
                {
                    // Refill the hold gauge so the next shotgun hit stays fully charged.
                    MpHoldAccessor(__instance) = curMg.reduce_mp;
                    MpOverholdAccessor(__instance) = 0f;
                    _retained = true;
                }
                else
                {
                    // The charge ran out and the held magic was destroyed: recreate it fully charged.
                    RecreateChargedMagic(__instance, pr, _shotgunSnapshotKind);
                }
            }
            catch (Exception ex)
            {
                Log.Error("[RapidAttack] error in digestShotgunHoldMp postfix", ex);
            }
        }

        private static void RunPrePrefix(PR __instance)
        {
            if (!_retained || _instance == null) return;
            if (!ReferenceEquals(__instance, AliceInCradleHack.utils.game.NelM2DBase.PlayerNoel)) return;

            try
            {
                if (IsAttackHeld(__instance)) return;

                // Attack key released: exhaust the retained charge.
                _retained = false;
                M2PrSkill skill = __instance.Skill;
                if (skill == null) return;

                MpHoldAccessor(skill) = 0f;
                MpOverholdAccessor(skill) = 0f;
                if (CurMgAccessor(skill) != null)
                {
                    skill.killHoldMagic(false, false, false);
                }
            }
            catch (Exception ex)
            {
                Log.Error("[RapidAttack] error in PR.runPre prefix", ex);
            }
        }

        private static bool IsLocalPlayerSkill(M2PrSkill skill)
        {
            return skill != null && ReferenceEquals(skill.Pr, AliceInCradleHack.utils.game.NelM2DBase.PlayerNoel);
        }

        private static bool IsAttackHeld(PR pr)
        {
            if (pr == null) return false;
            // AInputs[18] is the game's light-attack InputAction. It includes its current
            // keyboard/controller binding and therefore stays correct after rebinding.
            try
            {
                object key = InputKeyAccessor?.GetValue(null);
                Array inputs = InputActionsAccessor?.GetValue(key) as Array;
                object attackInput = inputs?.Length > 18 ? inputs.GetValue(18) : null;
                return attackInput != null && AttackInputIsOnMethod != null &&
                    (bool)AttackInputIsOnMethod.Invoke(attackInput, new object[] { false });
            }
            catch
            {
                return false;
            }
        }

        private static bool IsMagicHeld(M2PrSkill skill)
        {
            try
            {
                return skill != null && IsMagicOMethod != null &&
                    (bool)IsMagicOMethod.Invoke(skill, null);
            }
            catch
            {
                return false;
            }
        }

        private static void RecreateChargedMagic(M2PrSkill skill, PR pr, MGKIND kind)
        {
            if (kind == MGKIND.NONE) return;

            MagicItem mg = pr.NM2D.MGC.setMagic(pr, kind, MGHIT.AUTO);
            if (mg == null) return;

            CurMgAccessor(skill) = mg;
            skill.FlgSoftFall.Add("MAGIC");
            CursorAccessor(skill).initMagic(mg, true, false);
            MpHoldAccessor(skill) = mg.reduce_mp;
            MpOverholdAccessor(skill) = 0f;
            mg.t = mg.casttime; // fully charged
            _retained = true;
        }
    }
}
