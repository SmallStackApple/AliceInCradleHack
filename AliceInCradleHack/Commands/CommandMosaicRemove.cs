using System;
using HarmonyLib;

namespace AliceInCradleHack.Commands
{
    public class CommandMosaicRemove : Command
    {
        public override string Name => "mosaicremove";
        public override string Description => "Removes mosaic.";
        public override string Usage => "mosaicremove [patch/unpatch]";

        private bool isPatched = false;
        private Harmony harmony;
        public override void Execute(string[] args)
        {
            // make FnDrawMosaic(object XCon, ProjectionContainer JCon, Camera Cam) in nel.MosaicShower class in Assembly-CSharp.dll return false
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: mosaicremove [patch/unpatch]");
                return;
            }
            else
            {
                if (args[0] == "patch")
                {
                    if (isPatched)
                    {
                        Console.WriteLine("Mosaic removal is already patched.");
                        return;
                    }
                    harmony = new Harmony("aliceincradle.mosaicremove");
                    var original = AccessTools.Method("nel.MosaicShower:FnDrawMosaic");
                    var prefix = AccessTools.Method(typeof(CommandMosaicRemove), nameof(Prefix));
                    harmony.Patch(original, new HarmonyMethod(prefix));
                    isPatched = true;
                    Console.WriteLine("Mosaic removal patched.");
                }
                else if (args[0] == "unpatch")
                {
                    if (!isPatched)
                    {
                        Console.WriteLine("Mosaic removal is not patched.");
                        return;
                    }
                    harmony.UnpatchAll("aliceincradle.mosaicremove");
                    isPatched = false;
                    Console.WriteLine("Mosaic removal unpatched.");
                }
                else
                {
                    Console.WriteLine("Invalid argument. Usage: mosaicremove [patch/unpatch]");
                }
            }
        }

        private static bool Prefix()
        {
            return false; // Skip the original method
        }
    }
}
