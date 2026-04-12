using AliceInCradleHack.Utils.Client;
using System;
using System.Linq;

namespace AliceInCradleHack.Commands
{
    public class CommandNotify : Command
    {
        public override string Name => "notify";

        public override string Description => "Sends a notification message.";

        public override string Usage =>
            "notify <alert_type> <message>\n" +
            "alert_type: [NORMAL, GETITEM, CONSUMEITEM, MONEY, ALERT, ALERT_EGG, ALERT_EP, ALERT_EP2, ALERT_GRAY, ALERT_HUNGER, ALERT_BENCH, ALERT_PUPPET, ALERT_FATAL, ALERT_BARU]";
        public override void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage:" + Usage);
                return;
            }
            Notification.ShowNotificationByUILog(
                string.Join(" ", args.Skip(1)),
                Enum.TryParse(args[0], true, out nel.UILogRow.TYPE alertType) ? alertType : nel.UILogRow.TYPE.ALERT
            );
        }
    }
}