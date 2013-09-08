using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GPSServer.ServerCore.Connect
{
    internal class Connect : IDisposable
    {
        public DateTime LastActive { get; private set; }

        public string SessionIp { get; private set; }
        public dynamic ConnectProtocol { get; private set; }

        public Connect(string ip, byte[] initBuffer)
        {
            SessionIp = ip;
            LastActive = DateTime.Now;
            this.ConnectProtocol = Protocol.ProtocolManager.GetProtocol(initBuffer);
            this.ProcessMessege(initBuffer);

        }

        public void Dispose()
        {
           this. SessionIp = null;
            this.ConnectProtocol = null;
            GC.Collect();
        }

        public byte[] ProcessMessege(byte[] rowBuffer)
        {
            Monitor.Enter(this);
            #region ConvertToStandardBuffer
            var start = Array.IndexOf(rowBuffer, Byte.Parse("120"), 0, 2);
            var end = Array.LastIndexOf(rowBuffer, Byte.Parse("13"))+1;
            var buffer = new byte[end - start];
            Array.Copy(rowBuffer,start,buffer,0,end);
            #endregion

            var res = this.ConnectProtocol.ProcessMessege(buffer);

            LastActive = DateTime.Now;
            Monitor.Exit(this);
            return res;
        }

        public override bool Equals(object obj)
        {
            return obj.ToString() == ToString();
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
        ///     尝试添加一个新链接，如果链接已经存在则返回已存在的链接
        /// </summary>
        /// <param name="ip">链接的IP地址和端口</param>
        /// <param name="initBuffer">识别数据流</param>
        /// <returns>一个链接</returns>
        public Connect Add(string ip, byte[] initBuffer)
        {
            try
            {
                foreach (var connect in this.Where(connect => ip == connect.SessionIp))
                {
                    return connect;
                }
                var newConnect = new Connect(ip, initBuffer);
              this.  Add(newConnect);
                return newConnect;
            }
            catch (Exception ex)
            {
                
                throw new Exception("Can Not Match Content Or Establish!",ex);
            }

        }

        /// <summary>
        ///     用于回收超时的链接
        /// </summary>
        public void RecycleConnect()
        {
            while (true)
            {
                Monitor.Enter(this);
                //foreach (
                //    var connect in this.Where(connect => connect.LastActive < (DateTime.Now + new TimeSpan(0, 0, 5, 0)))
                //    )
                for (var index=0;index<this.Count;index++)
                {
                    this[index].Dispose();
                    Remove(this[index]);
                }
                Monitor.Exit(this);
                Thread.Sleep(new TimeSpan(0, 0, 5, 0));
            }

        }
    }
}