using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento
{
    // For 'using' to safely disable change tracking
    public class MementorLocker : IDisposable
    {
        internal static bool locked = false;
        public MementorLocker()
        {
            locked = true;
        }

        public void Dispose()
        {
            locked = false;
        }
    }
}
