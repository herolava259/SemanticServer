using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageClient
{
    public class StateClientBucket : IStateBucket
    {
        public StateClientBucket()
        {
            _errorSt = new Stack<TimeLineEvent> { };
            _loggingQ = new Queue<TimeLineEvent> ();
            _notifDict = new Dictionary<string, string> ();

        }
        private Stack<TimeLineEvent> _errorSt;

        private Queue<TimeLineEvent> _loggingQ;

        private Dictionary<string, string> _notifDict;
        public IEnumerable<string> ErrorStack => _errorSt.Select(c => c.ToString());

        public Dictionary<string, string> Notifications => _notifDict;

        public IEnumerable<string> LoggingQueue => _loggingQ.Select(c => c.ToString());

        public void AddError(string error)
        {
            _errorSt.Push(new TimeLineEvent
            {
                Note = error,
            });
        }

        public void AddExceptionError(Exception ex)
        {
            string innerMsg = "";

            innerMsg = ex?.InnerException?.Message + "\n";

            _errorSt.Push(new TimeLineEvent
            {
                Note = innerMsg + ex.Message,
            });
        }

        public void Logging(string msg)
        {
            _loggingQ.Enqueue(new TimeLineEvent
            {
                Note = msg,
            });
        }


        
        public string PopLatestError()
        {
            return _errorSt.Pop().ToString();
        }

        public void Reset()
        {
            _errorSt.Clear();
            _loggingQ.Clear();
            _notifDict.Clear();
        }

        public void WriteSomeThing(string title, string note)
        {
            _notifDict.Add(title, note);
        }

        public void ClearLog()
        {
            _loggingQ.Clear();
        }

        public void ClearError()
        {
            _errorSt.Clear();
        }

        public void ClearNotification()
        {
            _notifDict.Clear();
        }

        public bool NoLog()
        {
            return _errorSt.Count == 0 && _loggingQ.Count == 0;
        }
    }
}
