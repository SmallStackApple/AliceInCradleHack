using System;
using System.Reflection;

namespace AliceInCradleHack.Utils
{
    public static class Notification
    {
        // Using reflection to access the game's UI log system
        private static Type _uiLogRowType;
        private static Type _uiLogRowTypeEnum;
        private static FieldInfo _uiLogInstanceField;
        private static MethodInfo _addAlertMethod;

        public static Type uiLogRowType
        {
            get
            {
                if (_uiLogRowType == null)
                {
                    _uiLogRowType = typeof(nel.UILogRow);
                }
                return _uiLogRowType;
            }
        }

        public static Type uiLogRowTypeEnum
        {
            get
            {
                if (_uiLogRowTypeEnum == null && uiLogRowType != null)
                {
                    _uiLogRowTypeEnum = uiLogRowType.GetNestedType("TYPE", BindingFlags.Public);
                }
                return _uiLogRowTypeEnum;
            }
        }

        public static FieldInfo uiLogInstanceField
        {
            get
            {
                if (_uiLogInstanceField == null)
                {
                    var uiLogType = Type.GetType("nel.UILog, Assembly-CSharp");
                    if (uiLogType != null)
                    {
                        _uiLogInstanceField = uiLogType.GetField("Instance", BindingFlags.Public | BindingFlags.Static);
                    }
                }
                return _uiLogInstanceField;
            }
        }

        public static MethodInfo addAlertMethod
        {
            get
            {
                if (_addAlertMethod == null && uiLogInstanceField != null)
                {
                    _addAlertMethod = Type.GetType("nel.UILog, Assembly-CSharp").GetMethod("AddAlert", BindingFlags.Public | BindingFlags.Instance);
                }
                return _addAlertMethod;
            }
        }

        public static void ShowNotification(string message, NotificationType type = NotificationType.ALERT)
        {
            try
            {
                if (string.IsNullOrEmpty(message)) return;
                
                object uiLogInstance = uiLogInstanceField?.GetValue(null);
                if (uiLogInstance == null || addAlertMethod == null || uiLogRowTypeEnum == null)
                {
                    Console.WriteLine($"[AliceInCradleHack][Notification] Failed to get required reflection objects");
                    return;
                }

                addAlertMethod.Invoke(uiLogInstance, new object[] { message, Enum.Parse(uiLogRowTypeEnum, type.ToString(), true) });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AliceInCradleHack][Notification] ShowNotification exception: {ex.Message}");
            }
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