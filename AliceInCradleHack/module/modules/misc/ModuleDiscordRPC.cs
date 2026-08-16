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
        private DiscordRpcClient _rpcClient;

        public override void Enable()
        {
            var client = new DiscordRpcClient(DiscordApplicationId);
            try
            {
                client.Initialize();
                client.SetPresence(new RichPresence()
                {
                    Details = Details,
                    State = State
                });
            }
            catch
            {
                client.Dispose();
                throw;
            }

            _rpcClient = client;
        }

        public override void Disable()
        {
            if (_rpcClient == null) return;
            try
            {
                _rpcClient.ClearPresence();
            }
            finally
            {
                _rpcClient.Dispose();
                _rpcClient = null;
            }
        }
    }
}
