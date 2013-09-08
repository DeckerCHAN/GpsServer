using System;
using GPSServer.ServerCore;

namespace GPSServer.ServerConsoleVersion
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Initialization();
            var command="";
            Console.Write("EnterPort:");
             var myServer = new Server(Convert.ToInt16(Console.ReadLine()));
            myServer.ServerStarted += (i, o) => Console.WriteLine("Server Starded! Port:"+o.ToString());
            myServer.ServerStoped += (i, o) => Console.WriteLine("Server Stoped!");
            myServer.ServerError += (i, o) => Console.WriteLine("Server Error! Port:" + o.ToString());
            myServer.MessegeProcessed += (i, o) => Console.WriteLine(Environment.NewLine+"Messege Hearded:"+o.ToString());
            Console.WriteLine("Please Enter Your Command:");

            do
            {
               Console.Write('>');
                command = Console.ReadLine();
            
                    switch (command.Trim().ToLower())
                    {
                        case null:
                        {
                            Console.WriteLine("Your Command Is Empyt!");
                            break;
                        }
                        case "start":
                        {
                            myServer.StatrServer();
                            break;
                        }
                        case "stop":
                        {
                            myServer.StopServer();
                            break;
                        }
                        default:
                        {
                            Console.WriteLine("Unknown Command:"+command+"!");
                            break;
                        }
                    }
            

               
            } while (command.Trim().ToLower()!="exit");

        }

        static bool Initialization()
        {
            //TODO:检查文件完整性
            Console.WriteLine("Server  Initializating");

            Console.WriteLine("Server Finish Initializate");
            return true;
        }
    }
}
