using System;

namespace AliceInCradleHack.events
{
    public static class XxINEvents
    {
        public static event EventHandler<UpdateEventArgs> EventPreUpdate;
        public static event EventHandler<UpdateEventArgs> EventPostUpdate;

        public static void PreUpdate(object instance)
        {
            Invoke(EventPreUpdate, instance);
        }

        public static void PostUpdate(object instance)
        {
            Invoke(EventPostUpdate, instance);
        }

        private static void Invoke(EventHandler<UpdateEventArgs> handler, object instance)
        {
            if (handler == null) return;

            try
            {
                handler.Invoke(instance, new UpdateEventArgs(instance));
            }
            catch (Exception ex)
            {
                utils.client.Log.Error("XxINEvents handler exception", ex);
            }
        }

        public sealed class UpdateEventArgs : EventArgs
        {
            public object Instance { get; }

            public UpdateEventArgs(object instance)
            {
                Instance = instance;
            }
        }
    }
}
