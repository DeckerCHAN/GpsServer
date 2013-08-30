using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LumiSoft.Net;
using LumiSoft.Net.TCP;

namespace GPSServer.ServerCore
{
    public class ServerEventArgs : EventArgs
    {
        public string EventMessege;
        public ServerEventArgs(String eventMessege)
        {
            this.EventMessege = eventMessege;
        }

        public override string ToString()
        {
            return this.EventMessege;
        }
    }
    public class Server
    {
        TCP_Server<TCP_ServerSession> Core;
        private Object DataBase;
        //TODO:通过GET和SET加入对服务器的控制！
        //TODO:控制服务器ProcessMessege返回值

        public Server(int port)
        {
            this.Core = new TCP_Server<TCP_ServerSession>();

            try
            {
                this.Core.Started += (i, o) => this.OnServerStarted();
                this.Core.Stopped += (i, o) => this.OnServerStoped();
                this.Core.Error += CoreError;
                this.Core.SessionCreated += ProcessMessege;

                this.Core.Bindings = new[] { new IPBindInfo(Dns.GetHostEntry(String.Empty).HostName, BindInfoProtocol.TCP, IPAddress.Any, port) };


            }
            catch (Exception ex)
            {


            }

        }

        void CoreError(object sender, Error_EventArgs e)
        {
            OnServerError(e.Text);
        }

        private void ProcessMessege(object sender, TCP_ServerSessionEventArgs<TCP_ServerSession> e)
        {
            //TODO:将获得的内容放入

            new Thread(argSession =>
            {
                try
                {
                    var session = argSession as TCP_Session;
                    var msg = session.TcpStream.SourceStream;
                    var ip = session.RemoteEndPoint.ToString();
                    var char16 = new StringBuilder();
                    char16.Append(" FROM: " + ip + " MSG:");
                    while (true)
                    {
                        var temp = msg.ReadByte();
                        if (temp >= 0)
                        {
                            char16.Append(Convert.ToString(temp, 16).ToUpper().PadLeft(2, '0') + " ");
                        }
                        else
                        {
                            break;
                        }
                    }


                    // e.Session.TcpStream
                    this.OnMessegeProcessed(this.GetDateString() + char16.ToString());
                    var back = new TCP_Client();
                    back.TcpStream.Write(
                        new byte[] { 0x78, 0x78, 0x05, 0x01, 0x00, 0x01, 0xd9, 0xd9, 0x0d, 0x0a }, 0, 10);
                    back.Connect(session.RemoteEndPoint.Address.ToString(), session.RemoteEndPoint.Port);
                    //Another Way
                    //  new Socket(AddressFamily.HyperChannel, SocketType.Stream, ProtocolType.Tcp).Send(new byte[] {0x07});
                    //  session.TcpStream.Flush();
                    session.Disconnect();

                }
                catch (Exception ex)
                {
                    this.OnServerError(GetDateString() + ex.Message);
                }
            }).Start(e.Session);


        }
        #region ServerControler

        public void StatrServer()
        {

            if (!this.Core.IsRunning)
            {
                this.Core.Start();
            }
            else
            {
                this.Core.Restart();
            }

        }

        public void StopServer()
        {
            this.Core.Stop();
        }
        #endregion
        #region Events
        public event EventHandler MessegeProcessed = null;

        protected virtual void OnMessegeProcessed(String messege)
        {
            EventHandler handler = MessegeProcessed;
            if (handler != null) handler(this, new ServerEventArgs(messege));
        }

        public event EventHandler ServerStarted = null;

        protected virtual void OnServerStarted()
        {
            EventHandler handler = ServerStarted;
            if (handler != null) handler(this, new ServerEventArgs(this.Core.Bindings[0].HostName.ToString() + " " + this.Core.Bindings[0].Port.ToString() + " " + this.Core.Bindings[0].IP.ToString()));
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
            if (handler != null) handler(this, new EventArgs());
        }

        #endregion
        #region OtherMethods

        private string GetDateString()
        {
            return '[' + DateTime.Now.ToString("t") + ']';
        }
        #endregion

    }


}
