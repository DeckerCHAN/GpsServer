using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;
using GPSServer.ServerCore.Connect;
using GPSServer.ServerCore.Protocol;
using LumiSoft.Net;
using LumiSoft.Net.IO;
using LumiSoft.Net.TCP;
using SqlDal;


namespace GPSServer.ServerCore
{
    public class ServerEventArgs : EventArgs
    {
        public string EventMessege { get; set; }
        public ServerEventArgs(String eventMessege)
        {
            EventMessege = eventMessege;
        }

        public override string ToString()
        {
            return EventMessege;
        }
    }
    public class Server
    {
        private TCP_Server<TCP_ServerSession> _core { get; set; }
    
        private ConnectList _connects { get; set; }
        private ServerDatabaseControl DataBase { get; set; }

        //TODO:通过GET和SET加入对服务器的控制！
        //TODO:控制服务器ProcessMessege返回值

        public Server(int port)
        {
            _core = new TCP_Server<TCP_ServerSession>();
            _connects = new ConnectList();
            DataBase = new ServerDatabaseControl();
            _core.SessionIdleTimeout = ServerConfig.MaxSessionWaitTime;
            ProtocolManager.Init();
            try
            {
                _core.Started += (i, o) => OnServerStarted();
                _core.Stopped += (i, o) => OnServerStoped();
                _core.Error += CoreError;
                _core.SessionCreated += SendMessege;

                _core.Bindings = new[] { new IPBindInfo(Dns.GetHostEntry(String.Empty).HostName, BindInfoProtocol.TCP, IPAddress.Any, port) };
            }
            catch (Exception )
            {
            }
        }
        private void CoreError(object sender, Error_EventArgs e)
        {
            OnServerError(e.Text);
        }

        private void SendMessege(object sender, TCP_ServerSessionEventArgs<TCP_ServerSession> e)
        {
            //TODO:将获得的内容放入
           

            OnMessege(e.Session.ConnectTime + "Connect Established!");
          
            var connect = new Connect.Connect(e.Session, this.DataBase);
            connect.MessegeCatched += (i, o) => OnMessege(o.ToString());
            connect.ErrorOccured += (i, o) => OnServerError(o.ToString());
            this._connects.Add(connect);

            //new Thread(argSession =>
            //{
            //    try
            //    {
            //        var session = argSession as TCP_ServerSession;
            //        SmartStream msg = session.TcpStream;

            //        var consoleOutput = new StringBuilder();
            //        consoleOutput.Append(" FROM: " + session.RemoteEndPoint.ToString() + " MSG:");
            //        while (true)  
            //        {
            //            var buffer = new byte[16384];
            //            if (!msg.CanRead)
            //            {
            //                break;
            //            }
            //            msg.Read(buffer, 0, 16384);
            //            consoleOutput = new StringBuilder();
            //            consoleOutput.Append(" FROM: " + session.RemoteEndPoint.ToString() + " MSG:");
            //            for (int index = 0; index < 256; index++)
            //            {
            //                byte t = buffer[index];
            //                consoleOutput.Append(Convert.ToString(t, 16).ToUpper().PadLeft(2, '0') + " ");
            //            }
            //            var content = this._connects.Add(session.RemoteEndPoint.ToString(), buffer);
            //            try
            //            {
            //                this.DataBase.ExecuteCommand(content.SQLCommand(buffer));
            //                var res = content.SendMessege(buffer);
            //                session.TcpStream.Write(res, 0, res.Length);
            //                session.TcpStream.Flush();
            //                consoleOutput.Append("REPLY");
            //                foreach (var b in res)
            //                {
            //                    consoleOutput.Append(Convert.ToString(b, 16).ToUpper().PadLeft(2, '0') + " ");
            //                }
            //            }
            //            catch (Exception ex)
            //            {
            //                //DONE:添加单次链接处理异常
            //                OnServerError(GetDateString() + ex.Message + ex.ToString() + " Sum Of Connect:" + this.ThreadCount);

            //            }

            //            OnMessege(session.ConnectTime + consoleOutput.ToString() + " Sum Of Connect:" + this.ThreadCount);


            //        }

            //        //    e.Session.TcpStream


            //        //this.OnMessege(session.ConnectTime + char16.ToString());
            //        //var back = new TCP_Client();
            //        //back.TcpStream.Write(
            //        //    new byte[] { 0x78, 0x78, 0x05, 0x01, 0x00, 0x01, 0xd9, 0xdc, 0x0d, 0x0a }, 0, 10);
            //        //back.Connect(session.RemoteEndPoint.Address.ToString(), session.RemoteEndPoint.Port);
            //        //back.Disconnect();
            //        //Another Way
            //        //new Socket(AddressFamily.HyperChannel, SocketType.Stream, ProtocolType.Tcp).Send(new byte[] { 0x78, 0x78, 0x05, 0x01, 0x00, 0x01, 0xd9, 0xdc, 0x0d, 0x0a });
            //        //session.TcpStream.Flush();
            //        OnMessege(this.GetDateString() + "Connect Disconnected!");
            //        this.ThreadCount--;
            //        session.Disconnect();
            //    }
            //    catch (Exception ex)
            //    {
            //        OnServerError(GetDateString() + ex.Message + ex.ToString() + " Sum Of Connect:" + this.ThreadCount);
            //        this.ThreadCount--;
            //    }
            //}).Start(e.Session);
        }

        #region ServerControler

        public void StatrServer()
        {
            if (!_core.IsRunning)
            {
                _core.Start();
            }
            else
            {
                _core.Restart();
            }
        }

        public void StopServer()
        {
            _core.Stop();
        }

        public void LoadConfig(XmlDocument configDocument)
        {

        }
        #endregion

        #region Events
        public event EventHandler Messege = null;

        protected virtual void OnMessege(String messege)
        {
            EventHandler handler = Messege;
            if (handler != null) handler(this, new ServerEventArgs(messege));
        }

        public event EventHandler ServerStarted = null;

        protected virtual void OnServerStarted()
        {
            EventHandler handler = ServerStarted;
            if (handler != null)
                handler(this,
                    new ServerEventArgs(_core.Bindings[0].HostName + " " + _core.Bindings[0].Port + " " +
                                        _core.Bindings[0].IP));
        }

        public event EventHandler ServerStoped = null;

        protected virtual void OnServerStoped()
        {
            EventHandler handler = ServerStoped;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler ServerError = null;

        protected virtual void OnServerError(string errorMessege)
        {
            EventHandler handler = ServerError;
            if (handler != null) handler(this, new ServerEventArgs(errorMessege));
        }

        #endregion

        public void TestSqlConnect()
        {
            if (this.DataBase != null)
            {
                OnMessege(this.DataBase.TestConnect() );
            }
        }
    }

    public static  class ServerConfig
    {
        public static int MaxConsoleOutPutBufferLength = 256;
        public static int MaxInputBufferLength = 16484;
        public static int MaxSessionWaitTime = 400;
        public static int MaxConnectWaitTime = 400;

        public static void LoadConfig()
        {
            MaxConsoleOutPutBufferLength = 256;
            MaxInputBufferLength = 16484;
        }
    }
}