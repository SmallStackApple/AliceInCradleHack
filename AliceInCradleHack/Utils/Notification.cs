using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AliceInCradleHack.Utils
{
    public static class Notification
    {
        // Using reflection to access the game's UI log system
        private static readonly Type uiLogRowType = Type.GetType("nel.UILogRow, Assembly-CSharp");
        private static readonly Type uiLogRowTypeEnum = uiLogRowType.GetNestedType("TYPE", BindingFlags.Public);

        public static void ShowNotification(string message, NotificationType type = NotificationType.ALERT)
        {
            object uiLogInstance = Type.GetType("nel.UILog, Assembly-CSharp").GetField("Instance", BindingFlags.Public | BindingFlags.Static).GetValue(null);
            MethodInfo addAlertMethod = uiLogInstance.GetType().GetMethod("AddAlert", BindingFlags.Public | BindingFlags.Instance);
            addAlertMethod.Invoke(uiLogInstance, new object[] { message, Enum.Parse(uiLogRowTypeEnum, type.ToString(), true) });
        }

        public enum NotificationType
        {
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
        }
    }
}
