using AliceInCradleHack.utils.client;
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

        private static void Invoke(EventHandler<UpdateEventArgs> handlers, object instance)
        {
            if (handlers == null) return;

            var args = new UpdateEventArgs(instance);
            foreach (EventHandler<UpdateEventArgs> handler in handlers.GetInvocationList())
            {
                try
                {
                    handler.Invoke(instance, args);
                }
                catch (Exception ex)
                {
                    Log.Error("XxINEvents handler exception", ex);
                }
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
