using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPSServer.ServerCore
{
    internal class Device:IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    internal class DeviceList : List<Device>
    {
        public DeviceList()
        {
           
        }
        
    }
}
