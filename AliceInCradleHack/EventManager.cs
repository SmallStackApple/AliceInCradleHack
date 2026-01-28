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
            //Search and initialize all events
            var eventTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(Event)) && !type.IsAbstract);

            foreach (var eventType in eventTypes)
            {
                var eventInstance = (Event)Activator.CreateInstance(eventType);
                eventInstance.Initialize();
            }
        }
    }
}
