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
        public static string DefaltConnectString = "database=JMWEBGPS_II3;Server=10.217.168.69,1433;User=sa;Password=jmgps749892hujfejhwJKJI8;Persist Security Info=True";
        public static List<string> Log= new List<string>();
        public static void Init()
        {
            Log = new List<string>();
        }
        public string ConnectString { get; private set; }
        public ServerDatabaseControl(string connectString)
        {
            ConnectString = connectString;
        }

        public ServerDatabaseControl()
        {
            ConnectString = DefaltConnectString;
        }

        public string TestConnect()
        {
            try
            {
                SqlHelper.ExecuteNonQuery(this.ConnectString, CommandType.Text, "select null");
                return "Connect Success";
            }
            catch (Exception ex)
            {
                return "Connect Faild.Cause: "+ex.Message;
            }
          
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
                  //  Log.Add(ex.ToString());
                }
            }
        }

    }

}
