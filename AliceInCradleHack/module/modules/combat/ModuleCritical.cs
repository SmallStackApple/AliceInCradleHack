using AliceInCradleHack.module.settings;
using AliceInCradleHack.utils.client;
using AliceInCradleHack.utils.game;
using static AliceInCradleHack.events.DamageEvents;

namespace AliceInCradleHack.module.modules.combat
{
    public class ModuleCritical : Module
    {
        public override string Name => "Critical";
        public override string Description => "Boost player attack damage.";
        public override string Author => "SmallStackApple";
        public override string Version => "1.0.0";
        public override string Category => "Combat";

        public override SettingNode Settings { get; } = new SettingBuilder()
            .Add("Multiplier", "Damage multiplier", 2.0d)
            .Group("CriticalNotification", "Critical notification")
                .Add("EnableNotification", "Enable critical hit notification", true)
                .Add("NotificationText", "Text to display on critical hit.(%a:The damage;%m:The multiplier;%b:The damage after multiplier)", "SilenceFix >> Critical Notification. %a=>%b")
                .Back()
            .Build();

        public override void Enable()
        {
            HpDamage.EventPreNotPlayerGetDamageHandler += DoCriticalHit;
        }

        public override void Disable()
        {
            HpDamage.EventPreNotPlayerGetDamageHandler -= DoCriticalHit;
        }

        public override void Initialize()
        {
        }

        private void DoCriticalHit(object sender, HpDamage.PreDamageEventArgs e)
        {
            if (e.AttackInfo.AttackFrom.GetType() != Player.TypeNoel) return;

            int originalDamage = e.Val;
            double multiplier = (double)Settings.GetValueByPath("Multiplier");
            int newDamage = (int)(originalDamage * multiplier);
            e.Val = newDamage;

            if ((bool)Settings.GetValueByPath("CriticalNotification.EnableNotification"))
            {
                string notificationText = (string)Settings.GetValueByPath("CriticalNotification.NotificationText");
                notificationText = notificationText.Replace("%a", originalDamage.ToString())
                                                   .Replace("%m", multiplier.ToString())
                                                   .Replace("%b", newDamage.ToString());
                Notification.ShowNotificationByUILog(notificationText, nel.UILogRow.TYPE.ALERT);
            }
        }
    }
}
