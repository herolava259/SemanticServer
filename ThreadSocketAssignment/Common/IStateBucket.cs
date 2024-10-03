using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class TimeLineEvent
    {
        public DateTime OccurrTime { get; init; }

        public string Note { get; set; }

        public TimeLineEvent()
        {
            OccurrTime = DateTime.UtcNow;
        }

        public override string ToString()
        {
            return $"[{OccurrTime.ToString("dd/MM/yyyy hh:mm:ss tt")}]: {Note}";
        }
    }
    public interface IStateBucket
    {
        IEnumerable<string> ErrorStack { get; } 

        Dictionary<string, string> Notifications { get;}

        IEnumerable<string> LoggingQueue { get; }

        void AddError(string error);

        void AddExceptionError(Exception ex);

        string PopLatestError();

        void Logging(string msg);

        void WriteSomeThing(string title, string note);

        void Reset();

        void ClearLog();

        void ClearError();

        void ClearNotification();

        bool NoLog();


    }
}
