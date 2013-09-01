using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPSServer.ServerCore
{
    internal class Devices : IDisposable
    {

        public void Dispose()
        {
            throw new NotImplementedException();
        }

    }

    internal class DeviceList : List<Devices>
    {
        public DeviceList()
        {

        }

    }
}
