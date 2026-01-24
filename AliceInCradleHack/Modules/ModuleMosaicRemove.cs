using AliceInCradleHack.Commands;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;

namespace AliceInCradleHack.Modules
{
    public class ModuleMosaicRemove : Module
    {
        public override string Name => "MosaicRemove";
        public override string Description => "Removes mosaic from the game.";
        public override string Author => "SmallStackApple";
        public override string Version => "1.0.0";
        public override bool IsEnabled { get; set; } = false;
        public override Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();
        public override string Category { get; } = "Visuals";

        private const string NamespaceName = "aliceincradlehack.modules.mosaicremove";

        private readonly Harmony harmony = new Harmony(NamespaceName);
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
            harmony.UnpatchAll(NamespaceName);
            IsEnabled = false;
        }

        private static bool Prefix()
        {
            return false; // Skip the original method
        }
    }
}
