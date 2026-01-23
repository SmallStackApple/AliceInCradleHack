using AliceInCradleHack.Commands;
using HarmonyLib;
using System;
using System.Collections;

namespace AliceInCradleHack.Modules
{
    public class ModuleMosaicRemove : Module
    {
        public override string Name => "Mosaic Remove";
        public override string Description => "Removes mosaic from the game.";
        public override string Author => "SmallStackApple";
        public override string Version => "1.0.0";
        public override bool IsEnabled { get; set; } = false;
        public override ArrayList Settings { get; set; } = new ArrayList();
        public override string Category { get; } = "Visuals";

        private readonly Harmony harmony = new Harmony("aliceincradlehack.modules.mosaicremove");
        public override void Initialize(){}
        public override void Enable()
        {
            var original = AccessTools.Method("nel.MosaicShower:FnDrawMosaic");
            var prefix = AccessTools.Method(typeof(ModuleMosaicRemove), nameof(Prefix));
            harmony.Patch(original, new HarmonyMethod(prefix));
            IsEnabled = true;
        }

        public override void Disable()
        {
            // Logic to disable mosaic removal
            harmony.UnpatchAll("aliceincradlehack.modules.modulemosaicremove");
            IsEnabled = false;
        }

        private static bool Prefix()
        {
            return false; // Skip the original method
        }
    }
}
