using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MessageServer
{
    
    public class SimpleMessageServer:IDisposable
    {
        public const ulong MAXTIMESECOND = 1000000000;

        private Socket? _listener;

        private string _domain;

        private int _port;

        private static int _maxWaiting;

        private static int _maxServing = 20;

        private SessionManager _manager;

        private bool? isOnline = true;

        public void StopServing()
        {
            
            isOnline = false;
            
        }

        private Thread?[]? _servingWorkers;

        public void SetLimitWaiting(int maxWaiting = 10)
        {
            _maxWaiting = maxWaiting;
        }

        public void SetLimitServing(int maxServing) {
            _maxServing = maxServing;
            _servingWorkers = new Thread[maxServing];
        }
        public SimpleMessageServer(string domain = "localhost", int port = 11000)
        {
            _domain = domain;
            _port = port;
            _manager = SessionManager.GetInstance();
            _listener = default;
            _servingWorkers = default;
        }


        public bool Init(SocketType sockType = SocketType.Stream, ProtocolType protoType = ProtocolType.Tcp)
        {
            var host = Dns.GetHostEntry(_domain);
            var ip = host.AddressList.FirstOrDefault();
            if(ip == null)
            {
                Console.WriteLine("Cannot find ip with domain name!");
                return false;
            }


            var endpoint = new IPEndPoint(ip, _port);
            _listener = new Socket(ip.AddressFamily,sockType, protoType);

            _listener.Bind(endpoint);

            _listener.Listen(_maxWaiting);

            Console.WriteLine("Start listener socket successfully!");
            Console.WriteLine($"----------------Information Server--------------");
            Console.WriteLine($"Domain Name: {_domain}");
            Console.WriteLine($"Ip Address : {ip.ToString()}");
            Console.WriteLine($"Maximum of Client in Accepted Connection Queue: {_maxWaiting}");
            Console.WriteLine("-------------------------------------------------");

            
            return true;

        }

        public void DoListeningConnection()
        {
            //SimpleMessageSession? newSess = default;
            while (true)
            {
                var accepted = _listener?.Accept();
                
                _manager.AddAcceptedSocket(accepted);
                
                //Thread.Sleep(100);
            }
        }

        public void DoCoordinatingSession()
        {
            int numServingThread = 0;
            Thread? newThread = null;
            SimpleMessageSession? newSess = null;
            bool check = true;
            while (check)
            {
                // setting out of index of array
                int readyIdx = -1;
                // clean indexer in array that has Worker but shutdown and find available indexed to set new worker
                for(int i =0; i < _maxServing; i++)
                {
                    if (_servingWorkers[i] != null && !_servingWorkers[i].IsAlive)
                    {
                        _servingWorkers[i] = null;
                        readyIdx = i;
                        numServingThread--;
                    }
                    else if (_servingWorkers[i] == null)
                    {
                        readyIdx = i;
                    }
                }

                //readyIdx = 0;
                if(_manager.NumAcceptedClient > 0 && numServingThread < _maxServing)
                {
                    newThread = new Thread(DoServing);
                    
                    numServingThread++;
                    
                    newSess = _manager.InitSession(newThread.ManagedThreadId);
                    
                    newThread.Start((object)newSess);
                    
                }

                if(readyIdx > -1 && newThread != null)
                {
                    _servingWorkers[readyIdx] = newThread;
                }
                newThread = null;

                
            }

            for (int i = 0; i < _maxServing; ++i)
            {
                _servingWorkers[i]?.Join();
            }

        }

        public  void DoServing(Object? sessionObj)
        {
            if(sessionObj is not null)
            {
                SimpleMessageSession sess = (SimpleMessageSession)sessionObj ;

                sess?.Serving();
                sess?.Shutdown();
            }

            return;

        }
        public void Run()
        {
            var coordinatedThread = new Thread(DoCoordinatingSession);
            var listeningThread = new Thread(DoListeningConnection);

            

            coordinatedThread.Start();
            listeningThread.Start();

            ulong countingTime = 0;
            while (true)
            {
                _manager.PrintAllLog();
                Thread.Sleep(1000);
                countingTime++;

                if(countingTime > MAXTIMESECOND)
                {
                    break;
                }
            }
            coordinatedThread.Join();
            listeningThread.Join();
        }

        public void Dispose()
        {
            _listener?.Shutdown(SocketShutdown.Both);
            _listener?.Close();
        }
    }
}
