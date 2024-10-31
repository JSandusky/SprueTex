using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Commands
{
    public interface IQuickActionSource
    {
        CommandInfo[] GetCommands();
    }
}
