﻿using System;
using System.Net;
using System.Text;
using System.Threading;
using GPSServer.ServerCore.Connect;
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
        private TCP_Server<TCP_ServerSession> _core;
        private ConnectList _connects;
        private Object DataBase;

        //TODO:通过GET和SET加入对服务器的控制！
        //TODO:控制服务器ProcessMessege返回值

        public Server(int port)
        {
            _core = new TCP_Server<TCP_ServerSession>();
            _connects = new ConnectList();

            try
            {
                _core.Started += (i, o) => OnServerStarted();
                _core.Stopped += (i, o) => OnServerStoped();
                _core.Error += CoreError;
                _core.SessionCreated += ProcessMessege;

                _core.Bindings = new[] { new IPBindInfo(Dns.GetHostEntry(String.Empty).HostName, BindInfoProtocol.TCP, IPAddress.Any, port) };
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
            OnMessegeProcessed(e.Session.ConnectTime + "Connect Established!");



            new Thread(argSession =>
            {
                try
                {
                    var session = argSession as TCP_ServerSession;
                    SmartStream msg = session.TcpStream;

                    var char16 = new StringBuilder();
                    char16.Append(" FROM: " + session.RemoteEndPoint.ToString() + " MSG:");
                    byte[] buffer;
                    int repPoint = 0;
                    DateTime lastPoint = DateTime.Now;
                    while (true)
                    {
                        buffer = new byte[16384];
                        msg.Read(buffer, 0, 16384);
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
                        var connsct = this._connects.AddConnect(session.RemoteEndPoint.ToString(), buffer);
                        var res = connsct.ProcessMessege(buffer);
                        session.TcpStream.Write(res, 0, res.Length);
                        session.TcpStream.Flush();
                        OnMessegeProcessed(session.ConnectTime + char16.ToString());

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

        #region OtherMethods

        private string GetDateString()
        {
            return '[' + DateTime.Now.ToString("t") + ']';
        }

        #endregion
    }
}