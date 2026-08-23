using AliceInCradleHack.utils.client;
using AliceInCradleHack.utils.game;
using HarmonyLib;
using m2d;

namespace AliceInCradleHack.module.modules.movement
{
    public class ModuleInfiniteJump : Module
    {
        private readonly Harmony _harmony = new("aliceincradlehack.modules.movement.infinitejump");

        public ModuleInfiniteJump() : base("InfiniteJump", "Allow the player to jump without landing.", "Movement")
        {
        }

        public override void Enable()
        {
            var original = AccessTools.Method(typeof(M2Mover), "canJump");
            if (original == null)
            {
                Log.Error("InfiniteJump patch target m2d.M2Mover.canJump was not found.");
                return;
            }

            _harmony.Patch(
                original,
                prefix: new HarmonyMethod(typeof(ModuleInfiniteJump), nameof(CanJumpPrefix)));
        }

        public override void Disable()
        {
            _harmony.UnpatchAll(_harmony.Id);
        }

        private static bool CanJumpPrefix(M2Mover __instance, ref bool __result)
        {
            if (__instance != null && ReferenceEquals(__instance, NelM2DBase.PlayerNoel))
            {
                __result = true;
                return false;
            }

            return true;
        }
    }
}
