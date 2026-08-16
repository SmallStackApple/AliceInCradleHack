using AliceInCradleHack.config;
using HarmonyLib;
using m2d;
using nel;

namespace AliceInCradleHack.module.modules.visual.hypixel
{
    /// <summary>
    /// Displays Hypixel-style battle HUD text (countdown, victory, defeat)
    /// centered on the screen using the Minecraft font.
    /// </summary>
    public class ModuleHypixel : Module
    {
        public ModuleHypixel() : base("Hypixel", "Shows Hypixel-style battle text (3-2-1, VICTORY!, YOU DIED!) in the center of the screen.", "Visual")
        {
        }

        private readonly Harmony _harmony = new("aliceincradlehack.modules.visual.hypixel");

        public readonly RangedValue<float> TitleFontSize = new(48f, 12f, 200f, "px", "Font size of the main title.");
        public readonly RangedValue<float> SubtitleFontSize = new(24f, 8f, 120f, "px", "Font size of the subtitle.");
        public readonly RangedValue<float> Offset = new(0f, -600f, 600f, "px", "Vertical offset of the HUD from the screen center (positive moves down).");
        public readonly RangedValue<float> CountdownInterval = new(1f, 0.2f, 5f, "s", "Seconds each countdown digit is shown.");
        public readonly RangedValue<float> ResultDuration = new(4f, 1f, 20f, "s", "Seconds the victory / defeat message stays visible.");

        public override void Initialize()
        {
            TitleFontSize.OnChanged(_ => ApplySettings());
            SubtitleFontSize.OnChanged(_ => ApplySettings());
            Offset.OnChanged(_ => ApplySettings());
            CountdownInterval.OnChanged(_ => ApplySettings());
            ResultDuration.OnChanged(_ => ApplySettings());

            ApplySettings();
        }

        public override void Enable()
        {
            HypixelHud.EnsureCreated();

            _harmony.Patch(
                original: AccessTools.Method(typeof(M2LpSummon), nameof(M2LpSummon.openSummoner), new[] { typeof(M2Mover), typeof(IM2ManaWeedHitable), typeof(bool) }),
                prefix: new HarmonyMethod(typeof(ModuleHypixel), nameof(OpenSummonerPrefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(M2LpSummon), nameof(M2LpSummon.closeSummoner), new[] { typeof(bool), typeof(bool).MakeByRefType() }),
                postfix: new HarmonyMethod(typeof(ModuleHypixel), nameof(CloseSummonerPostfix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(GAMEOVER), nameof(GAMEOVER.activate), new[] { typeof(bool) }),
                prefix: new HarmonyMethod(typeof(ModuleHypixel), nameof(GameOverPrefix))
            );
        }

        public override void Disable()
        {
            _harmony.UnpatchAll(_harmony.Id);
            HypixelHud.Clear();
        }

        private void ApplySettings()
        {
            HypixelHud.TitleFontSize = TitleFontSize.Get();
            HypixelHud.SubtitleFontSize = SubtitleFontSize.Get();
            HypixelHud.Offset = Offset.Get();
            HypixelHud.CountdownInterval = CountdownInterval.Get();
            HypixelHud.ResultDuration = ResultDuration.Get();
        }

        private static void OpenSummonerPrefix()
        {
            HypixelHud.OnBattleStart();
        }

        private static void CloseSummonerPostfix(bool defeated)
        {
            if (defeated)
            {
                HypixelHud.OnVictory();
            }
            else
            {
                HypixelHud.OnBattleEnd();
            }
        }

        private static void GameOverPrefix()
        {
            HypixelHud.OnDefeat();
        }
    }
}
