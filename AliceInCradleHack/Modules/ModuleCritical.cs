using AliceInCradleHack.Events;
using AliceInCradleHack.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliceInCradleHack.Modules
{
    public class ModuleCritical : Module
    {
        public override string Name => "Critical";

        public override string Description => "Boost player attack damage.";

        public override string Author => "SmallStackApple";

        public override string Version => "1.0.0";

        public override bool IsEnabled { get; set; } = false;

        public override string Category => "Combat";

        public override SettingNode Settings { get; } = new SettingBuilder()
            .Add("Multiplier","Damage multiplier", 2.0d)
            .Group("CriticalNotification", "Critical notification")
                .Add("EnableNotification", "Enable critical hit notification", true)
                .Add("NotificationText", "Text to display on critical hit.(%a:The damage;%m:The multiplier;%b:The damage after multiplier)", "SilenceFix >> Critical Notification. %a=>%b")
                .Back()
            .Build();

        public override void Disable()
        {
            DamageEvents.EventPreEnemyGetDamageHandler -= DoCriticalHit;
        }

        public override void Enable()
        {
            DamageEvents.EventPreEnemyGetDamageHandler += DoCriticalHit;
        }

        public override void Initialize()
        {
        }

        private void DoCriticalHit(object sender, DamageEvents.PreDamageEventArgs e)
        {
            if(e.AttackInfo.GetType().GetField("AttackFrom").GetValue(e.AttackInfo).GetType() == Player.typeNoel)
            {
                int originalDamage = e.val;
                double multiplier = (double)Settings.GetValueByPath("Multiplier");
                int newDamage = (int)(originalDamage * multiplier);
                if (originalDamage > M2Attackable.GetHp(sender))
                {
                    newDamage = originalDamage;
                }
                else if (newDamage > M2Attackable.GetHp(sender))
                {
                    newDamage = M2Attackable.GetHp(sender);
                }
                e.val = newDamage;
                if ((bool)Settings.GetValueByPath("CriticalNotification.EnableNotification"))
                {
                    string notificationText = (string)Settings.GetValueByPath("CriticalNotification.NotificationText");
                    notificationText = notificationText.Replace("%a", originalDamage.ToString())
                                                       .Replace("%m", multiplier.ToString())
                                                       .Replace("%b", newDamage.ToString());
                    Notification.ShowNotification(notificationText, Notification.NotificationType.ALERT);
                }
            }
        }
    }
}
