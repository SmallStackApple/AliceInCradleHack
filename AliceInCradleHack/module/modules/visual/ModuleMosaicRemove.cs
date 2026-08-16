using AliceInCradleHack.utils.client;
using HarmonyLib;

namespace AliceInCradleHack.module.modules.visual
{
    public class ModuleMosaicRemove : Module
    {
        public ModuleMosaicRemove() : base("MosaicRemove", "Removes mosaic from the game.", "Visual")
        {
        }

        private readonly Harmony _harmony = new("aliceincradlehack.modules.visual.mosaicremove");

        public override void Initialize()
        {
        }

        public override void Enable()
        {
            var original = AccessTools.Method("nel.MosaicShower:FnDrawMosaic");
            var prefix = AccessTools.Method(typeof(ModuleMosaicRemove), nameof(FnDrawMosaicPrefix));
            if (original == null || prefix == null)
            {
                Log.Error("MosaicRemove patch target was not found.");
                return;
            }
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
