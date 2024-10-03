using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MessageClient
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var bucket = new StateClientBucket();

            using(var engine = new SimpleMessageClient(bucket))
            {

                ClientCLI cli = new ClientCLI(engine);

                await cli.Run();


            }
           
        }

        
    }
}