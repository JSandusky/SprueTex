using System;
using System.Net;
using WebSocket4Net;
using System.Xml;

namespace SprueKit
{
    /// <summary>
    /// Server that runs in the single instance program
    /// </summary>
    public class AppIPCServer
    {
        SuperSocket.WebSocket.WebSocketServer server_;

        public AppIPCServer()
        {
            try
            {
                server_ = new SuperSocket.WebSocket.WebSocketServer();
                var generalSettings = new IOCDependency<Settings.GeneralSettings>().Object;
                server_.Setup(generalSettings.IPCPort);
                server_.NewMessageReceived += Server__NewMessageReceived;
                server_.Start();
            }
            catch (Exception)
            {
                server_.Dispose();
                server_ = null;
                ErrorHandler.inst().Warning("Failed to start IPC server, some features may not function correctly");
            }
        }

        private void Server__NewMessageReceived(SuperSocket.WebSocket.WebSocketSession session, string value)
        {
            try
            {
                string msg = value;
                if (msg.StartsWith("OPEN "))
                {
                    msg = msg.Substring(5);
                    App.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Pages.LaunchScreen.OpenString(msg);
                    }));
                }
                else if (msg.StartsWith("GENTEX "))
                {
                    msg = msg.Substring("GENTEX ".Length);
                    //TODO: extract target node ID
                    //TODO: load XML graph
                }
            }
            catch (Exception ex)
            {
            }
        }
    }

    public class AppIPC
    {
        WebSocket socket_;

        public AppIPC(string sendData)
        {
            var generalSettings = new IOCDependency<Settings.GeneralSettings>().Object;
            socket_ = new WebSocket(string.Format("ws://127.0.0.1:{0}/", generalSettings.IPCPort));
            socket_.EnableAutoSendPing = true;
            socket_.NoDelay = true;
            socket_.AutoSendPingInterval = 50;
            socket_.Opened += _Opened;
            socket_.Closed += _Closed;
            socket_.Error += _Error;
            message_ = sendData;
            socket_.Open();
        }

        public AppIPC(string ipAddress, string port, string sendData)
        {

        }

        bool finished_ = false;
        string message_;

        public bool IsFinished { get { return finished_; } }

        void _Opened(object o, EventArgs args)
        {
            socket_.Send(string.Format("OPEN {0}", message_));
            socket_.Close();
            socket_.Dispose();
            finished_ = true;
        }

        void _Closed(object o, EventArgs args)
        {
            finished_ = true;
        }

        void _Error(object o, SuperSocket.ClientEngine.ErrorEventArgs args)
        {
            ErrorHandler.inst().Error(args.Exception.Message);
            finished_ = true;
        }

        public AppIPC()
        {
            
        }

        void _ReceiveMsg(object o, MessageReceivedEventArgs args)
        {
            try
            {
                string msg = args.Message;
                if (msg.StartsWith("OPEN "))
                {
                    msg = msg.Substring(5);
                    Pages.LaunchScreen.OpenString(msg);
                }
            }
            catch (Exception ex)
            {
            }
        }
    }

    public class WebQuery
    {
        public static string GetWebFile(string url, string purpose = "update data")
        {
            try
            {
                ErrorHandler.inst().Info(string.Format("Seeking {0} from the internet", purpose));
                using (var client = new WebClient())
                {
                    string dataString = client.DownloadString("https://raw.githubusercontent.com/SprueKit/SprueTex/master/UpdateTable.xml");
                    return dataString;
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.inst().Warning(string.Format("Failed to retrieve {0} from internet", purpose));
            }
            return "";
        }

        public static void GetAppMessages()
        {
            string msgData = GetWebFile("");
            if (!string.IsNullOrWhiteSpace(msgData))
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(msgData);
                var messages = doc.DocumentElement.SelectNodes("//message");
                foreach (XmlElement elem in messages)
                {
                    InfoMessage msg = new InfoMessage
                    {
                        ID = elem.GetIntAttribute("id"),
                        Text = elem.InnerText,
                        Cmd = elem.GetAttribute("cmd"),
                        Rejected = bool.Parse(elem.GetAttribute("canceled")),
                    };
                    if (elem.HasAttribute("expires"))
                        msg.Expiration = DateTime.Parse(elem.GetAttribute("expires"));

                    // add message takes care of default rejection
                    ((App)App.Current).AddMessage(msg);
                }
            }
        }
    }
}
