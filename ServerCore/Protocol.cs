using System;
using System.IO;
using System.Linq;
using System.Xml;
using GPSServer.ServerCore.Annotations;

namespace GPSServer.ServerCore.Protocol
{
    internal static class ProtocolManager
    {
        private static Protocol[] ProtocolList;

        public static void Init()
        {
            //TODO:读取协议的XML文件
            ProtocolList = new Protocol[Directory.GetFiles("Protocols").Length];
            string[] protocolPaths = Directory.GetFiles("Protocols");
            for (int i = 0; i < protocolPaths.Length; i++)
            {
                try
                {
                    var protocolFile = new FileInfo(protocolPaths[i]);

                    var xml = new XmlDocument();
                    xml.Load(protocolPaths[i]);

                    var symbolList = new Symbol[xml.SelectSingleNode("PROTOCOL/SYMBOLS").ChildNodes.Count];

                    for (int index = 0; index < symbolList.Length; index++)
                    {
                        symbolList[index] =
                            new Symbol(
                                Convert.ToInt32(
                                    xml.SelectNodes("PROTOCOL/SYMBOLS/SYMBOL")[index].Attributes["BYTEINDEX"].Value),
                                Convert.ToByte(xml.SelectNodes("PROTOCOL/SYMBOLS/SYMBOL")[index].InnerText, 16));
                    }
                    var subProtocolList =
                        new SubProtocol[xml.SelectSingleNode("PROTOCOL/SUBPROTOCOLS").ChildNodes.Count];
                    for (int index = 0; index < subProtocolList.Length; index++)
                    {
                        var reply = new byte[xml.SelectNodes("PROTOCOL/SUBPROTOCOLS/SUBPROTOCOL")[index].SelectNodes("REPLY/BYTE")
                            .Count];
                        for (int index1 = 0; index1 < reply.Length; index1++)
                        {
                            reply[index1] =
                                Convert.ToByte(xml.SelectNodes("PROTOCOL/SUBPROTOCOLS/SUBPROTOCOL")[index].SelectNodes("REPLY/BYTE")[index1].InnerText, 16);
                        }
                        subProtocolList[index] =
                            new SubProtocol(
                                xml.SelectNodes("PROTOCOL/SUBPROTOCOLS/SUBPROTOCOL")[index].Attributes["SUBNAME"].Value)
                            {
                                SymbolPoint = new Symbol(Convert.ToInt32(xml.SelectNodes("PROTOCOL/SUBPROTOCOLS/SUBPROTOCOL")[index].Attributes["BYTEINDEX"].Value), Convert.ToByte(xml.SelectNodes("PROTOCOL/SUBPROTOCOLS/SUBPROTOCOL")[index].Attributes["CODE"].Value, 16)),
                                Reply = reply
                            };
                    }
                    //TODO:读取协议名称
                    ProtocolList[i] = new Protocol(protocolFile.Name)
                    {
                        SymbolList = symbolList,
                        SubProtocolList = subProtocolList
                    };
                    //TODO:获取协议标准点
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public static Protocol GetProtocol([NotNull] byte[] firstBuffer)
        {
            //TODO:通过第一个TCP链接完成登录功能
            foreach (Protocol protocol in ProtocolList)
            {
                foreach (Symbol symbol in protocol.SymbolList)
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

        public static Protocol GetProtocol([NotNull] String protocolName)
        {
            return ProtocolList.FirstOrDefault(protocol => protocol.ProtocolName == protocolName);
        }
    }

    internal class Protocol
    {
        public Protocol(string protocolName)
        {
            ProtocolName = protocolName;
        }

        public SubProtocol[] SubProtocolList { get; internal set; }
        public string ProtocolName { get; internal set; }


        public Symbol[] SymbolList { get; internal set; }

        /// <summary>
        ///     得到协议中的子协议
        /// </summary>
        /// <param name="buffer">需要处理的字符流</param>
        /// <returns>对应的子协议</returns>
        public SubProtocol GetSubProtocol([NotNull] byte[] buffer)
        {
            foreach (SubProtocol subProtoco in SubProtocolList)
            {
                if (buffer[subProtoco.SymbolPoint.BufferIndex] == subProtoco.SymbolPoint.Value)
                {
                    return subProtoco;
                }
            }
            //TODO:如果匹配不到子协议的处理方式
            return null;
        }
    }

    internal class SubProtocol
    {
        /// <summary>
        ///     建立一个子协议
        /// </summary>
        /// <param name="subProtocoName">子协议的名字</param>
        public SubProtocol(string subProtocoName)
        {
            SubProtocoName = subProtocoName;
        }

        public string SubProtocoName { get; internal set; }
        public Symbol SymbolPoint { get; internal set; }
        public byte[] Reply { get; internal set; }
        public DataArea[] DataAreas { get; set; }

        /// <summary>
        ///     得到回复
        /// </summary>
        /// <returns>回复处理后的位串</returns>
        public byte[] GetReply()
        {
            return Reply;
        }

        /// <summary>
        ///     得到sql语句
        /// </summary>
        /// <returns>回复SQL语句用来处理</returns>
        public string GetSql()
        {
            //   return "EXEC "+this.SubProtocoName+

            return null;
        }

        internal struct DataArea
        {
            public int EndIndex;
            public string FildName;
            public int StartIndex;
        }
    }

    internal struct Symbol
    {
        public Symbol(int bufferIndex, byte value)
            : this()
        {
            BufferIndex = bufferIndex;
            Value = value;
        }

        public int BufferIndex { get; internal set; }
        public byte Value { get; internal set; }
    }
}