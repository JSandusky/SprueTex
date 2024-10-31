using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace PluginLib
{
    [Description("A service-plugin runs from the start.")]
    public interface IServicePlugin
    {
        [Description("Initialize the service plugin.")]
        void Start(IErrorPublisher publisher);

        [Description("Perform any necessary shutdown, return false to reject. False to reject shutdown, true to accept it.")]
        bool Stop(IErrorPublisher publisher);
    }
}
