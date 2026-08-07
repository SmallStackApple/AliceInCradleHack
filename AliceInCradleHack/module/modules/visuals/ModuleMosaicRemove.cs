using AliceInCradleHack.module.settings;
using HarmonyLib;

namespace AliceInCradleHack.module.modules.visuals
{
    public class ModuleMosaicRemove : Module
    {
        public override string Name => "MosaicRemove";
        public override string Description => "Removes mosaic from the game.";
        public override string Author => "SmallStackApple";
        public override string Version => "1.0.0";
        public override string Category => "Visuals";

        public override SettingNode Settings { get; } = new SettingGroup("MosaicRemove");

        private readonly Harmony _harmony = new("aliceincradlehack.modules.mosaicremove");

        public override void Initialize()
        {
        }

        public override void Enable()
        {
            var original = AccessTools.Method("nel.MosaicShower:FnDrawMosaic");
            var prefix = AccessTools.Method(typeof(ModuleMosaicRemove), nameof(FnDrawMosaicPrefix));
            _harmony.Patch(original, new HarmonyMethod(prefix));
        }

        public override void Disable()
        {
            _harmony.UnpatchAll(_harmony.Id);
        }

        // Skips the original mosaic drawing method.
        private static bool FnDrawMosaicPrefix()
        {
            return false;
        }
    }
}
