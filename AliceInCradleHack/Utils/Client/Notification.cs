using AliceInCradleHack.utils.game;
using System;

namespace AliceInCradleHack.utils.client
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
                Log.Error("ShowNotification exception", ex);
            }
        }
    }
}
