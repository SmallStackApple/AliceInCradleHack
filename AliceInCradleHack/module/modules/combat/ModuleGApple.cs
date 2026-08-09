using AliceInCradleHack.config;
using nel;

namespace AliceInCradleHack.module.modules.combat
{
    public class ModuleGApple : Module
    {
        public ModuleGApple() : base("GApple", "Auto use buff item", "Combat")
        {
        }

        public readonly RangedValue<int> MinHp = new("MinHP", 50, 0, 100, "%", "Minimum HP percentage to activate GApple.");

        public readonly RangedValue<double> Delay = new(2d, "Delay between GApple uses in seconds.") { Min = 0d };

        [SettingGroup("Notification", "Notification settings")]
        public readonly Value<bool> EnableNotification = new("Enable", true, "Enable notification when GApple is used.");

        [SettingGroup("Notification")]
        public readonly Value<string> NotificationText = new("GApple eaten!", "Text to display when GApple is used.(%hp:Current HP percentage)");

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
