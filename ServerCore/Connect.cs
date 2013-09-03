using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GPSServer.ServerCore.Connect
{
    internal class Connect : IDisposable
    {
        public Connect(string ip, byte[] initBuffer)
        {
            SessionIp = ip;
            LastActive = DateTime.Now;
        }

        public string SessionIp { get; private set; }
        public DateTime LastActive { get; private set; }

        public void Dispose()
        {
            SessionIp = null;
            GC.Collect();
        }

        public byte[] ProcessMessege(byte[] rowBuffer)
        {
            Monitor.Enter(this);
            var start = Array.IndexOf(rowBuffer, Byte.Parse("120"), 0, 2);
            var end = Array.LastIndexOf(rowBuffer, Byte.Parse("13"))+1;
            var buffer = new byte[end - start];
            Array.Copy(rowBuffer,start,buffer,0,end);

            LastActive = DateTime.Now;
            Monitor.Exit(this);
            return null;
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
            foreach (var connect in this.Where(connect => ip == connect.SessionIp))
            {
                return connect;
            }
            var newConnect = new Connect(ip, initBuffer);
            Add(newConnect);
            return newConnect;
        }

        /// <summary>
        ///     用于回收超时的链接
        /// </summary>
        public void RecycleConnect()
        {
            Monitor.Enter(this);
            foreach (
                var connect in this.Where(connect => connect.LastActive < (DateTime.Now + new TimeSpan(0, 0, 5, 0)))
                )
            {
                connect.Dispose();
                Remove(connect);
            }
            Monitor.Exit(this);
        }
    }
}