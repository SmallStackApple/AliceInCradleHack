using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliceInCradleHack
{
    public abstract class Event
    {
        public abstract event EventHandler Handler;
        public abstract void Initialize();
    }
}
