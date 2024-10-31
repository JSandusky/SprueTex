using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit
{
    /// <summary>
    /// Marked static void function will be executed on startup.
    /// Requires the class containing it to use AppLifecycleAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class LifecycleInitializeAttribute : System.Attribute
    {

    }

    /// <summary>
    /// Marked static bool function will be executed on shutdown.
    /// Return false to reject.
    /// Requires the class containing it to use AppLifecycleAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class LifecycleTerminateAttribute : System.Attribute
    {

    }
}
