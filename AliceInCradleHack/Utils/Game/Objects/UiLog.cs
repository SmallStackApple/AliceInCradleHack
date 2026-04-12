using System;
using System.ComponentModel;
using System.Reflection;

namespace AliceInCradleHack.Utils.Game.Objects
{
    public static class UILog
    {
        public static readonly Type typeUILog = typeof(nel.UILog);

        public static nel.UILog Instance => nel.UILog.Instance;

        public static void AddAlert(string t, nel.UILogRow.TYPE alert_type = nel.UILogRow.TYPE.ALERT) => Instance.AddAlert(t,alert_type);
    }
}
