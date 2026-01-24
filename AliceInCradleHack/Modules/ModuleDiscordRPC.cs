using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordRPC;

namespace AliceInCradleHack.Modules
{
    public class ModuleDiscordRPC : Module
    {
        public override string Name => "DiscordRPC";
        public override string Description => "Enables Discord Rich Presence integration.";
        public override string Author => "SmallStackApple";
        public override string Version => "1.0.0";
        public override bool IsEnabled { get; set; } = false;
        public override Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();
        public override string Category { get; } = "Misc";

        private const string DiscordApplicationId = "1462025663203774514";
        private static readonly DiscordRpcClient RPCClient = new DiscordRpcClient(DiscordApplicationId);

        public override void Initialize()
        {
            RPCClient.Initialize();
            Settings.Add("Ditails", "Playing Alice in Cradle");
            Settings.Add("State", "In Bug Wall");
        }
        public override void Enable()
        {
            RPCClient.SetPresence(new RichPresence()
            {
                Details = Settings["Ditails"].ToString(),
                State = Settings["State"].ToString(),
            });
            IsEnabled = true;
        }
        public override void Disable()
        {
            RPCClient.ClearPresence();
            IsEnabled = false;
        }
    }
}
