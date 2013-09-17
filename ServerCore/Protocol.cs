using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using GPSServer.ServerCore.Annotations;

namespace GPSServer.ServerCore.Protocol
{
    internal static class ProtocolManager
    {
        private static dynamic[] _protocolList;

        public static void Init()
        {

            _protocolList = new dynamic[Directory.GetFiles("Protocols", "*dll").Length];
            var protocolPaths = Directory.GetFiles("Protocols", "*dll");
            for (var i = 0; i < protocolPaths.Length; i++)
            {
                try
                {


                    var assembly = Assembly.LoadFrom(protocolPaths[i]);

                    var t = assembly.GetType( Path.GetFileNameWithoutExtension(protocolPaths[i]) + ".ProtocolManager");
                    _protocolList[i] = t.InvokeMember("GetProtocol", BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod, null, null, null);
                   

                }
                catch 
                {
                      throw;
                }
            }
        }

        public static dynamic GetProtocol([NotNull] byte[] firstBuffer)
        {
            //DONE:通过第一个TCP链接完成登录功能
            foreach (var protocol in _protocolList)
            {
                foreach (var symbol in protocol.SymbolList)
                {
                    if (firstBuffer[symbol.BufferIndex] != symbol.Value)
                    {





                        break;
                    }
                    if (Array.IndexOf(protocol.SymbolList, symbol) == protocol.SymbolList.Length - 1)
                    {
                        return protocol;
                    }
                }
            }
            throw new Exception("No Protocol Matched");
        }

        public static dynamic GetProtocol([NotNull] String protocolName)
        {
            return _protocolList.FirstOrDefault(protocol => protocol.ProtocolName == protocolName);
        }
    }
}