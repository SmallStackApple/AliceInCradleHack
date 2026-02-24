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

        public override SettingNode Settings { get; } = 
            new SettingBuilder()
            .Add("Details", "The details line of the Discord Rich Presence.","Playing Alice in Cradle")
            .Add("State", "The state line of the Discord Rich Presence.","In Bug Wall")
            .Build();

        public override string Category { get; } = "Misc";

        private const string DiscordApplicationId = "1462025663203774514";
        private static readonly DiscordRpcClient RPCClient = new DiscordRpcClient(DiscordApplicationId);

        public override void Initialize()
        {
            RPCClient.Initialize();
        }

        public override void Enable()
        {
            RPCClient.SetPresence(new RichPresence()
            {
                Details = (string)Settings.GetValueByPath("Details"),
                State = (string)Settings.GetValueByPath("State")
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
