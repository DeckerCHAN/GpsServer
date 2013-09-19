using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using LumiSoft.Net;
using LumiSoft.Net.TCP;

namespace GPSServer.ServerCore.Connect
{
    internal class MessegeEvnetArgs : EventArgs
    {
        public string Messege { get; private set; }

        public MessegeEvnetArgs(string messege)
        {
            Messege = messege;
        }

        public override string ToString()
        {
            return this.Messege;
        }
    }
    internal class Connect : IDisposable
    {
        private TCP_ServerSession _session { get; set; }

        private ServerDatabaseControl _database { get; set; }
        public DateTime LastActive { get; private set; }
        private string SessionIp { get; set; }
        public dynamic ConnectProtocol { get; private set; }
        public string DeviceID { get; private set; }
        public Thread ProcessThread { get; set; }

        public Connect(TCP_ServerSession session, ServerDatabaseControl database)
        {
            _session = session;
            _database = database;
            SessionIp = session.RemoteEndPoint.ToString();
            LastActive = DateTime.Now;
            this.ProcessThread = new Thread(() => ProcessMessege(session));
            this.ProcessThread.Start();
        }

        public void ProcessMessege(TCP_ServerSession session)
        {

            //byte[] rowBuffer;
            Monitor.Enter(this);
            var msg = session.TcpStream;
            while (true)
            {
                try
                {
                    var buffer = new byte[ServerConfig.MaxInputBufferLength];
                    if (!msg.CanRead)
                    {
                        break;
                    }
                    msg.Read(buffer, 0, ServerConfig.MaxInputBufferLength);
                    if (!msg.CanRead)
                    {
                        break;
                    }
                    Monitor.Enter(this);
                    //更新对象的最后活动时间
                    this.LastActive = DateTime.Now;
                    //控制台输出
                    var consoleOutput = new StringBuilder();
                    //将数据头部加入控制台
                    consoleOutput.Append(" FROM: " + session.RemoteEndPoint.ToString() + " MSG:");
                    //将流中的
                    for (var index = 0; index < ServerConfig.MaxConsoleOutPutBufferLength; index++)
                    {
                        var t = buffer[index];
                        consoleOutput.Append(Convert.ToString(t, 16).ToUpper().PadLeft(2, '0') + " ");
                    }
                    //本条数据异常捕获
                    try
                    {
                        if (this.ConnectProtocol == null)
                        {
                            this.ConnectProtocol = Protocol.ProtocolManager.GetProtocol(buffer);
                        }
                        if (this.DeviceID == null)
                        {
                            this.DeviceID = this.ConnectProtocol.GetDeviceID(buffer);
                        }
                        var res = this.ConnectProtocol.ProcessMessege(buffer);
                        msg.Write(res, 0, res.Length);
                        msg.Flush();
                        consoleOutput.Append("Replyed");
                        foreach (var b in res)
                        {
                            consoleOutput.Append(Convert.ToString(b, 16).ToUpper().PadLeft(2, '0') + " ");
                        }
                        //执行数据库存储过程
                        this._database.ExecuteCommand(this.ConnectProtocol.GetSQL(buffer, this.DeviceID));
                       
                    }

                    catch (Exception ex)
                    {

                        OnError("本条处理出现异常！" + ex.Message);
                    }
                    OnMessege(consoleOutput.ToString());
                    Monitor.Exit(this);
                }
                catch (System.Threading.ThreadAbortException ex)
                {
                    OnMessege("链接已经超时关闭！" + ex.Message);
                }
                catch (Exception ex)
                {

                    OnError("总处理线程出现异常！" + ex.ToString());
                    break;

                }

            }
            //  var res = this.ConnectProtocol.ProcessMessege(rowBuffer);


            //  return res;
        }

        public event EventHandler MessegeCatched = null;
        public void OnMessege(string messege)
        {
            EventHandler handler = MessegeCatched;
            if (handler != null) handler(this, new MessegeEvnetArgs(messege));
        }
        public event EventHandler ErrorOccured = null;
        public void OnError(string errorMessege)
        {
            EventHandler handler = ErrorOccured;
            if (handler != null) handler(this, new MessegeEvnetArgs(errorMessege));
        }


        public void Dispose()
        {
            this.SessionIp = null;
            this.ConnectProtocol = null;
            _session.Disconnect();
            this.ProcessThread.Abort();
            GC.Collect();
        }


    }

    internal class ConnectList : List<Connect>
    {
        private readonly Thread Recycle;

        public ConnectList()
        {
            Recycle = new Thread(RecycleConnect);
            Recycle.Start();
        }



        /// <summary>
        ///     用于回收超时的链接
        /// </summary>
        public void RecycleConnect()
        {
            while (true)
            {
                Monitor.Enter(this);

                for (var index = 0; index < this.Count; index++)
                {
                    if ((DateTime.Now - this[index].LastActive) > new TimeSpan(0, 0, 0, 20))
                    {
                        this[index].Dispose();
                        Remove(this[index]);
                    }
                }
                Monitor.Exit(this);
                Thread.Sleep(new TimeSpan(0, 0, 0, ServerConfig.MaxConnectWaitTime));
            }

        }
    }
}