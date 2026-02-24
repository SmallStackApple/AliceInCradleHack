using AliceInCradleHack.Utils;
using System;
using System.Linq;

namespace AliceInCradleHack.Commands
{
    public class CommandNotify : Command
    {
        public override string Name => "notify";

        public override string Description => "Sends a notification message.";

        public override string Usage => 
            "notify <alert_type> <message>\n"+
            "alert_type: [NORMAL, GETITEM, CONSUMEITEM, MONEY, ALERT, ALERT_EGG, ALERT_EP, ALERT_EP2, ALERT_GRAY, ALERT_HUNGER, ALERT_BENCH, ALERT_PUPPET, ALERT_FATAL, ALERT_BARU]";
        public override void Execute(string[] args)
        {
            //invoke nel.UILog "public UILogRow AddAlert(string t, UILogRow.TYPE alert_type = UILogRow.TYPE.ALERT)"
            /*
                enum nel.UILogRow.TYPE
                NORMAL,
			    GETITEM,
			    CONSUMEITEM,
			    MONEY,
			    ALERT,
			    ALERT_EGG,
			    ALERT_EP,
			    ALERT_EP2,
			    ALERT_GRAY,
			    ALERT_HUNGER,
			    ALERT_BENCH,
			    ALERT_PUPPET,
			    ALERT_FATAL,
			    ALERT_BARU
            */
            if (args.Length == 0)
            {
                Console.WriteLine("Usage:"+Usage);
                return;
            }
            Notification.ShowNotification(
                string.Join(" ", args.Skip(1)),
                Enum.TryParse<Notification.NotificationType>(args[0], true, out var type) ? type : Notification.NotificationType.ALERT
            );
        }
    }
}