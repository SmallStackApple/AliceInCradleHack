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
            DamageEvents.Initialize();
        }
    }
}
