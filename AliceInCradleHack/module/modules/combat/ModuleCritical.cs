using AliceInCradleHack.config;
using AliceInCradleHack.utils.client;
using AliceInCradleHack.utils.game;
using static AliceInCradleHack.events.DamageEvents;

namespace AliceInCradleHack.module.modules.combat
{
    public class ModuleCritical : Module
    {
        public ModuleCritical() : base("Critical", "Boost player attack damage.", "Combat")
        {
        }

        public readonly RangedValue<double> Multiplier = new(2.0d, "Damage multiplier") { Min = 0.1, Max = 10.0 };

        [SettingGroup("CriticalNotification", "Critical notification")]
        public readonly Value<bool> EnableNotification = new(true, "Enable critical hit notification");

        [SettingGroup("CriticalNotification")]
        public readonly Value<string> NotificationText = new(
            "SilenceFix >> Critical Notification. %a=>%b",
            "Text to display on critical hit.(%a:The damage;%m:The multiplier;%b:The damage after multiplier)");

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
            if (!ReferenceEquals(e.AttackInfo.AttackFrom, NelM2DBase.PlayerNoel)) return;

            int originalDamage = e.Val;
            double multiplier = Multiplier;
            int newDamage = (int)(originalDamage * multiplier);
            e.Val = newDamage;

            if (EnableNotification)
            {
                string notificationText = NotificationText;
                notificationText = notificationText.Replace("%a", originalDamage.ToString())
                                                   .Replace("%m", multiplier.ToString())
                                                   .Replace("%b", newDamage.ToString());
                Notification.ShowNotificationByUILog(notificationText, nel.UILogRow.TYPE.ALERT);
            }
        }
    }
}
