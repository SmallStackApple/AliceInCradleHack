using AliceInCradleHack.module.settings;
using nel;

namespace AliceInCradleHack.module.modules.combat
{
    public class ModuleGApple : Module
    {
        public override string Name => "GApple";
        public override string Description => "Auto use buff item";
        public override string Author => "SmallStackApple";
        public override string Version => "1.0.0";
        public override string Category => "Combat";

        public override SettingNode Settings { get; } =
            new SettingBuilder()
            .Add("MinHP", "Minimum HP percentage to activate GApple.", 50)
            .Add("Delay", "Delay between GApple uses in seconds.", 2d)
            .Group("Notification", "Notification settings")
                .Add("Enable", "Enable notification when GApple is used.", true)
                .Add("NotificationText", "Text to display when GApple is used.(%hp:Current HP percentage)", "GApple eaten!")
                .Back()
            .Build();

        private PRNoel Player => utils.game.SceneGame.PrNoelInstance;

        private UseItemSelector UseItemSelector => utils.game.UseItemSelector.Instance;

        private UseItemSelector.ItCell[] ACell => utils.game.UseItemSelector.ACell;

        public override void Enable()
        {
        }

        public override void Disable()
        {
        }

        public override void Initialize()
        {
        }

        private void Eat(UseItemSelector.ItCell cell)
        {
            PR pr = UseItemSelector.IMNG.Mp.getKeyPr() as PR;

            if (Player == null || UseItemSelector == null || ACell == null || !cell.Itm.useable || pr == null) return;

            int grade = cell.getGrade();
            ItemStorage inventory = UseItemSelector.IMNG.getInventory();

            if (cell.Info.getCount(grade) <= 0 || !pr.is_alive) return;
            int num = cell.Itm.Use(pr, inventory, grade, pr);
        }
    }
}
