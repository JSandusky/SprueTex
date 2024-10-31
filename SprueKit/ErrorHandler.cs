using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

using System.Collections.ObjectModel;

namespace SprueKit
{
    public class LogItem
    {
        public string Time { get; set; }
        public string Text { get; set; }
        public SolidColorBrush TextColor { get; set; }
    }

    /// <summary>
    /// Receives errors and is periodically checked for a need to display an error dialog
    /// Use to prevent exceptions from bringing down the program, but still provide a notice that
    /// things have not gone according to plan
    /// 
    /// \todo Errors should probably be written to a log
    /// </summary>
    public class ErrorHandler : PluginLib.IErrorPublisher
    {
        static ErrorHandler inst_;
        List<string> messages_ = new List<string>();

        public ObservableCollection<LogItem> Items { get; set; } = new ObservableCollection<LogItem>();

        public ErrorHandler()
        {
            inst_ = this;
        }

        public static ErrorHandler inst()
        {
            if (inst_ == null)
                new ErrorHandler();
            return inst_;
        }

        public bool Check()
        {
            if (messages_.Count > 0)
            {
                return true;
            }
            return false;
        }

        public string GetMessage()
        {
            string msg = messages_[0];
            messages_.RemoveAt(0);
            return msg;
        }

        public void Error(Exception ex)
        {
            String msg = String.Format("{0}\r\n\r\n{1}", ex.Message, ex.StackTrace);
            PublishError(msg, 2);
            messages_.Add(msg);
        }

        public void Info(string text)
        {
            PublishError(text, 0);
        }

        public void Warning(string text)
        {
            PublishError(text, 1);
        }

        public void Error(string text)
        {
            PublishError(text, 2);
        }

        public void Debug(string text)
        {
            PublishError(text, 3);
        }

        public void PublishError(Exception ex)
        {
            Error(ex);
        }

        public void PublishError(string msg, int level)
        {
            App.Current.Dispatcher.Invoke(() => {
                while (Items.Count > 100)
                    Items.RemoveAt(0);
                LogItem newItem = new SprueKit.LogItem { Text = msg, Time = DateTime.Now.ToString(), TextColor = new SolidColorBrush(Colors.White) };
                switch (level)
                {
                    case 0:
                        break;
                    case 1:
                        newItem.TextColor = new SolidColorBrush(Colors.Orange);
                        break;
                    case 2:
                        newItem.TextColor = new SolidColorBrush(Colors.Red);
                        break;
                    case 3:
                        newItem.TextColor = new SolidColorBrush(Colors.Magenta);
                        break;
                }
                Items.Add(newItem);
            });
            if (level == 2)
                messages_.Add(msg);
        }
    }
}
