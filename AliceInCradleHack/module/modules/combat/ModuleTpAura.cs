using AliceInCradleHack.config;
using AliceInCradleHack.utils.client;
using HarmonyLib;
using m2d;
using nel;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using XX;

namespace AliceInCradleHack.module.modules.combat
{
    public enum TargetClassFilterMode
    {
        Disabled,
        Whitelist,
        Blacklist
    }

    /// <summary>
    /// Port of Kaleidoscopic.Hacks.Neuvilette ("忿怒的报偿 / TP-Aura") rewritten
    /// for the module system. While a fully-charged White Arrow is held, teleports
    /// the player next to the nearest target every few frames and fires an
    /// empowered burst at it.
    /// </summary>
    public class ModuleTpAura : Module
    {
        public ModuleTpAura() : base("TpAura", "Teleport to the nearest target and fire an empowered White Arrow.", "Combat")
        {
        }

        public readonly RangedValue<int> FireIntervalFrames = new("FireIntervalFrames", 5, 1, 60, "frames", "Frames between arrow bursts.");
        public readonly RangedValue<float> TeleportDistance = new("TeleportDistance", 5.25f, 0.5f, 12f, "", "Distance to stop from the target when teleporting.");
        public readonly RangedValue<float> HorizontalSpeed = new("HorizontalSpeed", 4f, 0f, 12f, "", "Initial horizontal speed of the white arrow.");
        public readonly RangedValue<float> VerticalSpeed = new("VerticalSpeed", 2f, 0f, 12f, "", "Initial vertical speed of the white arrow.");
        public readonly Value<bool> TargetEnemy = new("TargetEnemy", false, "Target NelEnemy and its subclasses.");
        public readonly EnumChoiceValue<TargetClassFilterMode> ClassFilterMode = new("ClassFilterMode", TargetClassFilterMode.Disabled, "How the target class list is applied.");
        public readonly StringListValue TargetClassPatterns = new("TargetClassPatterns", null, "Class names or wildcard patterns used by the target filter.");

        private readonly Harmony harmony = new("aliceincradlehack.modules.combat.tpaura");

        private static ModuleTpAura _instance;
        private static readonly Random _rng = new Random();

        public override void Initialize()
        {
        }

        public override void Enable()
        {
            _instance = this;
            harmony.Patch(
                AccessTools.Method(typeof(PR), nameof(PR.runPre)),
                prefix: new HarmonyMethod(typeof(ModuleTpAura), nameof(RunPrePrefix))
            );
        }

        public override void Disable()
        {
            _instance = null;
            harmony.UnpatchAll(harmony.Id);
        }

        private static void RunPrePrefix(PR __instance)
        {
            ModuleTpAura module = _instance;
            if (module == null || __instance == null) return;

            try
            {
                module.Process(__instance);
            }
            catch (Exception ex)
            {
                Log.Error("[TpAura] error in PR.runPre prefix", ex);
            }
        }

        private void Process(PR pr)
        {
            M2PrSkill skill = pr.Skill;
            if (skill == null) return;

            MagicItem curMagic = skill.getCurMagic();
            if (curMagic == null || skill.getChantCompletedRatio() < 1f || curMagic.kind != MGKIND.WHITEARROW)
                return;

            if (IN.totalframe % FireIntervalFrames.Get() != 0)
                return;

            Map2d mp = pr.Mp;
            if (mp == null) return;

            FireBurst(pr, skill, curMagic, FindNearestTarget(mp, pr));
        }

        private M2Mover FindNearestTarget(Map2d mp, PR pr)
        {
            M2Mover[] movers = mp.getVectorMover();
            if (movers == null || movers.Length == 0) return null;

            PRNoel localNoel = AliceInCradleHack.utils.game.NelM2DBase.PlayerNoel;
            float px = pr.x;
            float py = pr.y;

            M2Mover nearest = null;
            float nearestDistSq = float.PositiveInfinity;

            foreach (M2Mover mover in movers)
            {
                if (mover == null || mover.destructed || ReferenceEquals(mover, localNoel) || !MatchesClassFilter(mover)) continue;

                float dx = mover.x - px;
                float dy = mover.y - py;
                float distSq = dx * dx + dy * dy;
                if (distSq < nearestDistSq)
                {
                    nearestDistSq = distSq;
                    nearest = mover;
                }
            }

            return nearest;
        }

        private bool MatchesClassFilter(M2Mover mover)
        {
            if (TargetEnemy.Get() && typeof(NelEnemy).IsAssignableFrom(mover.GetType()))
                return true;

            TargetClassFilterMode mode = ClassFilterMode.Get();
            if (mode == TargetClassFilterMode.Disabled) return true;

            bool matches = false;
            string typeName = mover.GetType().Name;
            string fullName = mover.GetType().FullName;
            foreach (string pattern in TargetClassPatterns.Items)
            {
                if (WildcardMatches(typeName, pattern) || WildcardMatches(fullName, pattern))
                {
                    matches = true;
                    break;
                }
            }

            return mode == TargetClassFilterMode.Whitelist ? matches : !matches;
        }

        private static bool WildcardMatches(string value, string pattern)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(pattern)) return false;
            string regex = "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$";
            return Regex.IsMatch(value, regex, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        private void FireBurst(PR pr, M2PrSkill skill, MagicItem curMagic, M2Mover target)
        {
            float angle = (float)(_rng.NextDouble() * 2.0 * Math.PI);
            float cosA = (float)Math.Cos(angle);
            float sinA = (float)Math.Sin(angle);
            float scale = (float)Math.Sin(3.0 * angle) * 0.1f + 1f;
            float forward = TeleportDistance.Get() * scale;
            float vxMag = HorizontalSpeed.Get() * scale;
            float vyMag = VerticalSpeed.Get() * scale;

            float targetX = target != null ? target.x : pr.x + cosA;
            float targetY = target != null ? target.y : pr.y + sinA;

            skill.PtcVar("cx", pr.x).PtcVar("cy", pr.y).PtcVar("time", 12f);
            skill.PtcSTTimeFixed("burst_prepare", 0f, PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.FOLLOW_C);

            float vx = -vxMag * cosA;
            float vy = vyMag * sinA;

            if (target != null)
                pr.moveBy(targetX - pr.x - forward * cosA, targetY - pr.y + forward * sinA, true);

            pr.NM2D.Cam.setQuake(40f, 20, 0f, 0);

            MagicItem magic = curMagic.createNewMagic(null, MGKIND.WHITEARROW, vx, vy, false);
            if (magic == null) return;

            magic.reduce_mp = 20f;
            magic.run(1f);
            for (int i = 0; i < 100 && magic.phase < 2; i += 10)
                magic.run(10f);
            magic.sa = (float)Math.Atan2(vy - 2.5f, -vx);
        }
    }
}
