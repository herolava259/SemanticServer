using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MessageServer
{
    public class SessionManager
    {
        private readonly IStateBucket _bucket;

        private readonly List<Guid> _recycleSessionIds;

        private readonly List<Guid> _runningSessionIds;

        private readonly Dictionary<Guid, SimpleMessageSession> _sessions;

        private readonly Queue<Socket> _waitingSocketQueue;

        private readonly Mutex _queueMutex;

        private static SessionManager? _instance;

        public int NumAcceptedClient { get { 
                return _waitingSocketQueue.Count();
            } }
        private SimpleMessageSession CreateSimpleMessageSession()
        {
            var session = new SimpleMessageSession();

            session.NotifyError += (s, msg) =>
            {
                Monitor.Enter(_bucket);
                _bucket.AddError($"\n---------------Thread {s._threadId}-------------\n");
                _bucket.AddError(msg);
                _bucket.AddError($"\n--------------------------------------------------\n");
                Monitor.Exit(_bucket);
            };

            session.NotifyInfo += (s, title, info) =>
            {
                Monitor.Enter(_bucket);
                _bucket.WriteSomeThing(title, info);
                Monitor.Exit(_bucket);
            };

            session.LoggingInfo += (s, info) =>
            {
                Monitor.Enter(_bucket);
                _bucket.Logging($"\n---------------Thread {s._threadId}-------------\n");
                _bucket.Logging(info);
                _bucket.Logging($"\n--------------------------------------------------\n");
                Monitor.Exit(_bucket);
            };

            session.NotifyException += (s, ex) =>
            {
                Monitor.Enter(_bucket);
                _bucket.AddExceptionError(ex);
                Monitor.Exit(_bucket);
            };

            session.NotifyShutdown += (s) =>
            {
                Monitor.Enter(_recycleSessionIds);
                _recycleSessionIds.Add(session.Id);
                Monitor.Exit(_recycleSessionIds);

                Monitor.Enter(_runningSessionIds);
                _runningSessionIds.Remove(session.Id);
                Monitor.Exit(_runningSessionIds);
                
            };

            return session;

        }

        public SimpleMessageSession InitSession(int threadId)
        {
            SimpleMessageSession sess;
            _queueMutex.WaitOne();
            var accepted = _waitingSocketQueue.Dequeue();
            _queueMutex.ReleaseMutex();
            if(_recycleSessionIds.Count == 0)
            {
                sess = CreateSimpleMessageSession();
                
                _sessions.Add(sess.Id, sess);

            }
            else
            {
                sess = _sessions[_recycleSessionIds[0]];
                Monitor.Enter(_recycleSessionIds);
                _recycleSessionIds.Remove(sess.Id);
                Monitor.Exit(_recycleSessionIds);
            }

            sess.SetConnection(accepted);
            sess.SetThreadId(threadId);
            
            Monitor.Enter(_runningSessionIds);
            _runningSessionIds.Add(sess.Id);
            Monitor.Exit(_runningSessionIds);

            return sess;
        }


        public static SessionManager GetInstance()
        {
            if (_instance == null)
            {
                _instance = new SessionManager();

            }
            return _instance;
        }
        private SessionManager()
        {
            _recycleSessionIds = new List<Guid>();
            _sessions = new Dictionary<Guid, SimpleMessageSession>();
            _bucket = new StateServerBucket();
            _sessions = new();
            _runningSessionIds = new();
            _waitingSocketQueue = new();

            _queueMutex = new();
        }

        public void AddAcceptedSocket(Socket? s)
        {
            if(s is not null)
            {
                _queueMutex.WaitOne();
                _waitingSocketQueue.Enqueue(s);
                _queueMutex.ReleaseMutex();
            }
        }
        public void PrintAllLog()
        {
            StringBuilder message = new StringBuilder();
            Monitor.Enter(_bucket);
            if(!_bucket.NoLog())
            {
                Console.WriteLine("Check");
                message.Append("\n-------------Log---------------------\n");
                if (_bucket.LoggingQueue.Count() == 0)
                {
                    message.Append("No information to Log");
                }
                else
                {
                    message.AppendJoin(".\n", _bucket.LoggingQueue.ToArray());
                    message.Append("\n-----------------------------------\n\n");
                }
                message.Append("\n---------------Error-----------------\n");
                if (_bucket.ErrorStack.Count() == 0)
                {
                    message.Append("No error!");
                }
                else
                {
                    message.AppendJoin(".\n", _bucket.ErrorStack.ToArray());
                    message.Append("\n-----------------------------------\n\n");
                }
                
            }
            _bucket.ClearError();
            _bucket.ClearLog();
            Monitor.Exit(_bucket);

            var ct = message.ToString();

            if(!String.IsNullOrEmpty(ct))
            {
                Console.WriteLine(message.ToString());
            }
        }

    }
}
