using System;
using System.Net;
using System.Text;
using System.Threading;
using LumiSoft.Net;
using LumiSoft.Net.IO;
using LumiSoft.Net.TCP;

namespace GPSServer.ServerCore
{
    public class ServerEventArgs : EventArgs
    {
        public string EventMessege;

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
        private readonly TCP_Server<TCP_ServerSession> Core;
        private Object DataBase;
        //TODO:通过GET和SET加入对服务器的控制！
        //TODO:控制服务器ProcessMessege返回值

        public Server(int port)
        {
            Core = new TCP_Server<TCP_ServerSession>();

            try
            {
                Core.Started += (i, o) => OnServerStarted();
                Core.Stopped += (i, o) => OnServerStoped();
                Core.Error += CoreError;
                Core.SessionCreated += ProcessMessege;

                Core.Bindings = new[]
                {new IPBindInfo(Dns.GetHostEntry(String.Empty).HostName, BindInfoProtocol.TCP, IPAddress.Any, port)};
            }
            catch (Exception ex)
            {
            }
        }

        private void CoreError(object sender, Error_EventArgs e)
        {
            OnServerError(e.Text);
        }

        private void ProcessMessege(object sender, TCP_ServerSessionEventArgs<TCP_ServerSession> e)
        {
            //TODO:将获得的内容放入
            OnMessegeProcessed(e.Session.ConnectTime + "Start Connect!");
            new Thread(argSession =>
            {
                try
                {
                    var session = argSession as TCP_ServerSession;
                    SmartStream msg = session.TcpStream;

                    var char16 = new StringBuilder();
                    char16.Append(" FROM: " + session.RemoteEndPoint.ToString() + " MSG:");

                    //var buffer = new byte[16384];
                    //msg.Read(buffer, 0, 16384);


                    //foreach (var t in buffer)
                    //{
                    //    if (t > 0)
                    //    {
                    //        char16.Append(Convert.ToString(t, 16).ToUpper().PadLeft(2, '0') + " ");
                    //    }
                    //    else
                    //    {
                    //        break;
                    //    }
                    //}


                    //while (true)
                    //{
                    //    var temp = msg.ReadByte();
                    //    if (temp >= 0)
                    //    {
                    //        char16.Append(Convert.ToString(temp, 16).ToUpper().PadLeft(2, '0') + " ");
                    //    }
                    //    else
                    //    {
                    //        break;
                    //    }
                    //}
                    byte[] buffer;
                    int repPoint = 0;
                    DateTime lastPoint = DateTime.Now;
                    while (true)
                    {
                        buffer = new byte[16384];
                        msg.Read(buffer, 0, 16384);


                        if (buffer[0] == 0 && buffer[1] == 0 && buffer[2] == 0 && buffer[3] == 0 && buffer[4] == 0)
                        {
                            if (repPoint > 5)
                            {
                                OnMessegeProcessed(session.ConnectTime + "Disconnect");
                                session.Disconnect();
                                break;
                            }
                            if ((DateTime.Now - lastPoint).Ticks < 1000000)
                            {
                                repPoint++;
                            }
                            lastPoint = DateTime.Now;
                        }
                        else
                        {
                            char16 = new StringBuilder();
                            char16.Append(" FROM: " + session.RemoteEndPoint.ToString() + " MSG:");
                            foreach (byte t in buffer)
                            {
                                if (t > 0)
                                {
                                    char16.Append(Convert.ToString(t, 16).ToUpper().PadLeft(2, '0') + " ");
                                }
                                else
                                {
                                    break;
                                }
                            }
                            session.TcpStream.Write(
                                new byte[] {0x78, 0x78, 0x05, 0x01, 0x00, 0x01, 0xd9, 0xdc, 0x0d, 0x0a}, 0, 10);
                            session.TcpStream.Flush();
                            OnMessegeProcessed(session.ConnectTime + char16.ToString());
                        }
                    }

                    //    e.Session.TcpStream


                    //this.OnMessegeProcessed(session.ConnectTime + char16.ToString());
                    //var back = new TCP_Client();
                    //back.TcpStream.Write(
                    //    new byte[] { 0x78, 0x78, 0x05, 0x01, 0x00, 0x01, 0xd9, 0xdc, 0x0d, 0x0a }, 0, 10);
                    //back.Connect(session.RemoteEndPoint.Address.ToString(), session.RemoteEndPoint.Port);
                    //back.Disconnect();
                    //Another Way
                    //new Socket(AddressFamily.HyperChannel, SocketType.Stream, ProtocolType.Tcp).Send(new byte[] { 0x78, 0x78, 0x05, 0x01, 0x00, 0x01, 0xd9, 0xdc, 0x0d, 0x0a });
                    //session.TcpStream.Flush();

                    //session.Disconnect();
                }
                catch (Exception ex)
                {
                    OnServerError(GetDateString() + ex.Message);
                }
            }).Start(e.Session);
        }

        #region ServerControler

        public void StatrServer()
        {
            if (!Core.IsRunning)
            {
                Core.Start();
            }
            else
            {
                Core.Restart();
            }
        }

        public void StopServer()
        {
            Core.Stop();
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
            if (handler != null)
                handler(this,
                    new ServerEventArgs(Core.Bindings[0].HostName + " " + Core.Bindings[0].Port + " " +
                                        Core.Bindings[0].IP));
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

        #region OtherMethods

        private string GetDateString()
        {
            return '[' + DateTime.Now.ToString("t") + ']';
        }

        #endregion
    }
}