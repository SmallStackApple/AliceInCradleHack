using AliceInCradleHack.Utils.Game.Objects;
using System;

namespace AliceInCradleHack.Utils.Client
{
    public static class Notification
    {
        public static void ShowNotificationByUILog(string message, nel.UILogRow.TYPE type = nel.UILogRow.TYPE.ALERT)
        {
            try
            {
                if (string.IsNullOrEmpty(message)) return;

                UILog.AddAlert(message, type);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AliceInCradleHack][Notification] ShowNotification exception: {ex.Message}");
            }
        }
    }
}