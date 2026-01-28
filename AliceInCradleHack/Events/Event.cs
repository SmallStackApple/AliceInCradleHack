using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliceInCradleHack
{
    // Utility container for event related helpers
    public static class Event
    {
        // Common EventArgs for integer events
        public class IntEventArg : EventArgs
        {
            public int Value { get; private set; }
            public IntEventArg(int value)
            {
                Value = value;
            }
        }
    }
}
