using Common.CommunicationModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Common.Utilities;

namespace MessageClient
{
    public class ClientCLI
    {
        private readonly SimpleMessageClient _logic;

        public ClientCLI(SimpleMessageClient logic)
        {
            _logic = logic;
        }

        public async Task Run()
        {
            try
            {
                Console.WriteLine("Start Logic engine.");
                _logic.Init();

                Console.WriteLine("Connecting to server");
                await _logic.ConnectAsync();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Init engine is failed!");
                Console.WriteLine(ex.ToString());
                return;
            }
            Console.WriteLine("You connected to server successfully");
            Console.WriteLine("-----------------Welcome to Message Management System------------");
            Loop();

        }

        public void ShowLogFromLogicLayer(string triggerName)
        {
            Console.WriteLine($"Logging this {triggerName} process");
            Console.WriteLine("-----------------Log---------------");
            _logic.GetLoggingBucket().LoggingQueue.ToList()
                                                  .ForEach(x => Console.WriteLine(x));
            _logic.GetLoggingBucket().ClearLog();

            Console.WriteLine("----------------Error------------");
            if (_logic.GetLoggingBucket().ErrorStack.Any())
            {
                _logic.GetLoggingBucket().ErrorStack.ToList()
                                                  .ForEach(x => Console.WriteLine(x));
            }
            else
            {
                Console.WriteLine("Not found error.");
            }
        }
        private void Loop()
        {
            string inp;
            int key = 0;

            bool check = true;
            while(key != 1)
            {
                Console.WriteLine(@"Please enter key to do action you want: 
1. Quit Session and shutdown program
2. Send Message
3. Ping to Server");

                inp = Console.ReadLine();

                if(!Int32.TryParse(inp, out key))
                {
                    Console.WriteLine("You entered wrong key that is not interger. Please enter key correct follow guide.");
                    continue;
                }

                switch (key)
                {
                    case 1:
                        {
                            Console.WriteLine("You chosen quit session and shutdown Program.");
                            Console.WriteLine("The computer is shutting down the program sequentially!");
                            Console.WriteLine("1.Request to the server to end session.");
                            check = _logic.RequestQuitSession();
                            ShowLogFromLogicLayer("Quit");

                            if (check)
                            {
                                Console.WriteLine("Request to server to quit session successfully");

                            }
                            else
                            {
                                Console.WriteLine("Request to server to quit session is failed");

                                Console.WriteLine("Error within this quit process");

                                Console.WriteLine("Do you want to shut down immedatetly but not sequentially?");

                                inp = "";
                                Console.Write("Yes(Y) / No (N): ");

                                inp = Console.ReadLine();
                                while (inp != "Y" && inp != "N")
                                {
                                    Console.WriteLine("Wrong Key. Please enter key correctly follow to guide");
                                    Console.Write("Yes(Y) / No (N): ");

                                    inp = Console.ReadLine();
                                }

                                if(inp == "Y")
                                {
                                    key = 1;
                                }
                                else
                                {
                                    key = 0;
                                }

                            }


                            break;
                        }
                    case 2:
                        {
                            Console.WriteLine("------------Sending Message--------------");
                            Console.WriteLine("You have requested to send message.Please enter in the following format");
                            Console.WriteLine(@"
1.Title: Example: Nice weather today
2.UserName: Example: Farrer
3.EmailAddress: Example: Farrer.Le@avepoint.com
4.Content: Today is a beautiful day, Clear sky, a little sunshine and a light breeze with the breath of the sea.
");
                            Console.WriteLine("-----------------------------------------");
                            MessageDTO messageDTO = new();
                            var rgx = new Regex(ProtocolConstant.EmailPattern, RegexOptions.Compiled);
                            Console.Write("Title: ");
                            inp = Console.ReadLine();
                            messageDTO.Title = inp;
                            Console.Write("UserName: ");
                            inp = Console.ReadLine();
                            messageDTO.UserName = inp;

                            Console.Write("Email: ");
                            inp = Console.ReadLine();

                            while(!rgx.IsMatch(inp))
                            {
                                Console.WriteLine("Please enter the correct email in the format.");
                                Console.Write("Email: ");
                                inp = Console.ReadLine();
                            }
                            messageDTO.EmailAddress = inp;

                            Console.Write("Content: ");
                            inp = Console.ReadLine();

                            messageDTO.Content = inp;


                            check = _logic.RequestSendMessage(messageDTO);
                            ShowLogFromLogicLayer("Sending Message");
                            if (check)
                            {
                                Console.WriteLine("Sending a message to server is succeed");
                            }
                            else
                            {
                                Console.WriteLine("Sending a message to server is failed");
                            }

                            break;
                        }
                    case 3:
                        {
                            Console.WriteLine("Ready Ping to server!");
                            _logic.RequestConnect();
                            break;
                        }
                    default:
                        {
                            Console.WriteLine("Wrong key! I must enter key following guide above");
                            break;
                        }
                }

                Console.WriteLine("Thank you for using my software. See you later!");
                Console.WriteLine("------------------------------------------------");

            }
        }

    }
}
