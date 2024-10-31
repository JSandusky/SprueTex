using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace PluginLib
{
    public class ErrorLevels
    {
        [Description("Will appear as white informational text")]
        public static readonly int INFO = 0;

        [Description("Will appear as orange caution text")]
        public static readonly int WARNING = 1;

        [Description("Will appear as red error text")]
        public static readonly int ERROR = 2;

        [Description("Will appear as magenta debug")]
        public static readonly int DEBUG = 3;
    }

    [Description("Interface is used to publish messages to the message log")]
    public interface IErrorPublisher
    {
        [Description("Publishes an exception as an error")]
        void PublishError(Exception ex);

        [Description("Publishes a generic message with the specified level")]
        void PublishError(string msg, int level);

        [Description("Writes a message at the informational level")]
        void Info(string msg);

        [Description("Writes a message at the warning level")]
        void Warning(string msg);

        [Description("Writes a message at the error level")]
        void Error(string msg);

        [Description("Writes a message at the debug level")]
        void Debug(string msg);
    }
}
