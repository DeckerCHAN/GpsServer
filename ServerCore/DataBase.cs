using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlDal;

namespace GPSServer.ServerCore
{

    internal class ServerDatabaseControl
    {
        public static List<string> Log;
        public static void Init()
        {
            Log = new List<string>();
        }
        public string ConnectString { get; private set; }
        public ServerDatabaseControl(string connectString)
        {
            ConnectString = connectString;
        }

        public void ExecuteCommand(string[] command)
        {

            foreach (var subCommand in command)
            {
                try
                {
                    SqlHelper.ExecuteNonQuery(this.ConnectString, CommandType.Text, subCommand);
                }
                catch (Exception ex)
                {
                    //TODO::保存数据库执行错误
                    //throw new Exception("DataBaseProcessError", ex);
                    Log.Add(ex.ToString());
                }
            }
        }

    }

}
