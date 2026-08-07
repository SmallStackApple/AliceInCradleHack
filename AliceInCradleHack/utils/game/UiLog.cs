using System;

namespace AliceInCradleHack.utils.game
{
    public static class UILog
    {
        public static readonly Type TypeUILog = typeof(nel.UILog);

        public static nel.UILog Instance => nel.UILog.Instance;

        public static void AddAlert(string t, nel.UILogRow.TYPE alertType = nel.UILogRow.TYPE.ALERT) => Instance.AddAlert(t, alertType);
    }
}
