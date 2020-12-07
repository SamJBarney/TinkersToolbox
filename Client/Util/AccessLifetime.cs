using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinkersToolbox.Client.Util
{
    class AccessLifetime<T>
    {
        private long lastAccess;
        private T value;

        public AccessLifetime(T obj)
        {
            value = obj;
            lastAccess = Now;
        }

        public bool IsValid(long lifespan)
        {
            return Age < lifespan;
        }

        public T Value {
            get {
                lastAccess = Now;
                return value;
            }
        }

        public long Age { get { return Now - lastAccess; } }

        private long Now { get { return DateTimeOffset.Now.Ticks / TimeSpan.TicksPerSecond; } }
    }
}
