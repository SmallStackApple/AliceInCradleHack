using AliceInCradleHack.Events;
using AliceInCradleHack.Utils;
using System;
using System.Collections.Generic;
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
            if(e.AttackInfo.GetType().GetField("AttackFrom").GetValue(e.AttackInfo).GetType() == Player.typeNoel && !(e.val == M2Attackable.GetMaxHp(sender)))
            {
                int originalDamage = e.val;
                e.val = (int)(e.val * (double)Settings.GetValueByPath("Multiplier"));
                if ((bool)Settings.GetValueByPath("CriticalNotification.EnableNotification"))
                {
                    double multiplier = (double)Settings.GetValueByPath("Multiplier");
                    int newDamage = (int)(originalDamage * multiplier);
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
