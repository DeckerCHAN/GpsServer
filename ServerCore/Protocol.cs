using GPSServer.ServerCore.Annotations;

namespace GPSServer.ServerCore.Protocol
{
    internal static class ProtocolManager
    {
        private static Protocol[] ProtocolList;

        public static void Init()
        {
            int n = 3;
            //TODO:读取协议的XML文件
            ProtocolList = new Protocol[n];
            for (int i = 0; i < ProtocolList.Length; i++)
            {
                string protocolName = "testName";
                //TODO:读取协议名称
                ProtocolList[i] = new Protocol(protocolName);
                //TODO:获取协议标准点
                ProtocolList[i].SymbolList = new[] { new Symbol(1, 0x78), new Symbol(2, 0x78) };
            }
        }
    }

    internal class Protocol
    {
        public Protocol(string protocolName)
        {
            ProtocolName = protocolName;
        }

        public Symbol[] SymbolList { get; set; }
        public string ProtocolName { private get; set; }
    }

    internal struct Symbol
    {
        public int BufferIndex;
        public byte Value;

        public Symbol(int bufferIndex, byte value)
        {
            BufferIndex = bufferIndex;
            Value = value;
        }
    }
}