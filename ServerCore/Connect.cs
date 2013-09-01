using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GPSServer.ServerCore.Connect
{
    internal class Connect : IDisposable
    {
        private Thread SessionThread = null;
        public string SessionIp { get; private set; }
        public DateTime LastActive { get; private set; }

        public Connect(string ip, byte[] initBuffer)
        {
            SessionIp = ip;
            LastActive = DateTime.Now;
        }

        public void Dispose()
        {
            this.SessionThread.Abort();
            this.SessionThread = null;
            this.SessionIp = null;
            GC.Collect();
        }

        public byte[] ProcessMessege(byte [] buffer)
        {
            return null;
        }

    }

    internal class ConnectList : List<Connect>
    {
        public ConnectList():base()
        {
          
        }
        /// <summary>
        /// 尝试添加一个新链接，如果链接已经存在则返回已存在的链接
        /// </summary>
        /// <param name="ip">链接的IP地址和端口</param>
        /// <param name="initBuffer">识别数据流</param>
        /// <returns>一个链接</returns>
        public Connect AddConnect(string ip, byte[] initBuffer)
        {
            foreach (var connect in this)
            {
                if (ip == connect.SessionIp)
                {
                    return connect;
                }
            }
            var newConnect = new Connect(ip, initBuffer);
            this.Add(newConnect);
            return newConnect;
        }





    }
}
