using AliceInCradleHack.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliceInCradleHack
{
    public class EventManager
    {
        private static Lazy<EventManager> _instance = new Lazy<EventManager>(() => new EventManager());
        public static EventManager Instance => _instance.Value;

        public void Initialize()
        {
            // Search and initialize all static event classes.
            // Static classes in C# are marked as abstract and sealed.
            var eventTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsClass && type.IsAbstract && type.IsSealed)
                .Where(type => type.GetMethod("Initialize", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static, null, Type.EmptyTypes, null) != null);

            foreach (var eventType in eventTypes)
            {
                var init = eventType.GetMethod("Initialize", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                init.Invoke(null, null);
            }
        }
    }
}
