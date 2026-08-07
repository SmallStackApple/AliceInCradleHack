using AliceInCradleHack.module.settings;
using DiscordRPC;

namespace AliceInCradleHack.module.modules.misc
{
    public class ModuleDiscordRPC : Module
    {
        public override string Name => "DiscordRPC";
        public override string Description => "Enables Discord Rich Presence integration.";
        public override string Author => "SmallStackApple";
        public override string Version => "1.0.0";
        public override string Category => "Misc";

        public override SettingNode Settings { get; } =
            new SettingBuilder()
            .Add("Details", "The details line of the Discord Rich Presence.", "Playing Alice in Cradle")
            .Add("State", "The state line of the Discord Rich Presence.", "In Bug Wall")
            .Build();

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
                Details = (string)Settings.GetValueByPath("Details"),
                State = (string)Settings.GetValueByPath("State")
            });
        }

        public override void Disable()
        {
            _rpcClient.ClearPresence();
        }
    }
}
