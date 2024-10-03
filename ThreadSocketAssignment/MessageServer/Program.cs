using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MessageServer
{
    public class Program
    {
        static void Main(string[] args)
        {
            
            using(var server = new SimpleMessageServer())
            {
                server.Init();
                server.SetLimitWaiting(2);
                server.SetLimitServing(2);
                Thread.CurrentThread.IsBackground = true;
                server.Run();
            }

        }

        
    }
}