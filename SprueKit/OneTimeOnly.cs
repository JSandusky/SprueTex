using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit
{
    /// <summary>
    /// Utility class for maintaining a value used for tracking a singular ocurrence of a need to do something.
    /// </summary>
    public class OneTimeOnly
    {
        bool isOnce_ = true;

        /// <summary>
        /// Checks (and dirties) the single time tracker.
        /// </summary>
        public bool Once
        {
            get
            {
                bool ret = isOnce_;
                isOnce_ = false;
                return ret;
            }
        }

        public void Reset()
        {
            isOnce_ = true;
        }
    }
}
