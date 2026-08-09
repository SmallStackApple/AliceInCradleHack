using AliceInCradleHack.config;
using DiscordRPC;

namespace AliceInCradleHack.module.modules.misc
{
    public class ModuleDiscordRPC : Module
    {
        public ModuleDiscordRPC() : base("DiscordRPC", "Enables Discord Rich Presence integration.", "Misc")
        {
        }

        public readonly Value<string> Details = new("Playing Alice in Cradle", "The details line of the Discord Rich Presence.");

        public readonly Value<string> State = new("In Bug Wall", "The state line of the Discord Rich Presence.");

        private const string DiscordApplicationId = "1462025663203774514";
        private static readonly DiscordRpcClient _rpcClient = new(DiscordApplicationId);

        public override void Initialize()
        {
            _rpcClient.Initialize();
        }

        public override void Enable()
        {
            _rpcClient.SetPresence(new RichPresence()
            {
                Details = Details,
                State = State
            });
        }

        public override void Disable()
        {
            _rpcClient.ClearPresence();
        }
    }
}
