using nel;

namespace AliceInCradleHack.Modules
{
    public class ModuleGApple : Module
    {
        public override string Name => "GApple";

        public override string Description => "Auto use buff item";

        public override string Author => "SmallStackApple";

        public override string Version => "1.0.0";

        public override bool IsEnabled { get; set; } = false;

        public override string Category => "Combat";

        public override SettingNode Settings =>
            new SettingBuilder()
            .Add("MinHP", "Minimum HP percentage to activate GApple.", 50)
            .Add("Delay", "Delay between GApple uses in seconds.", 2d)
            .Group("Notification", "Notification settings")
                .Add("Notify", "Enable notification when GApple is used.", true)
                .Add("NotificationText", "Text to display when GApple is used.(%hp:Current HP percentage)", "GApple eaten!")
                .Back()
            .Build();

        private PRNoel player => Utils.Game.Objects.SceneGame.PrNoelInstance;

        private UseItemSelector useItemSelector => Utils.Game.Objects.UseItemSelector.Instance;

        private UseItemSelector.ItCell[] ACell => Utils.Game.Objects.UseItemSelector.ACell;

        public override void Disable()
        {
        }

        public override void Enable()
        {
        }

        public override void Initialize()
        {
        }

        private void eatGApple()
        {
            foreach(var cell in ACell)
            {
            }
        }
    }
}