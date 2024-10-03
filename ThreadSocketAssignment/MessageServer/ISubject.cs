using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageServer
{
    public delegate void NotifyError(SimpleMessageSession session, string error);

    public delegate void NotifyInfo(SimpleMessageSession session, string title, string info);

    public delegate void NotifyException(SimpleMessageSession session, Exception ex);

    public delegate void LoggingInfo(SimpleMessageSession session, string msg);

    public delegate void NotifyShutdown(SimpleMessageSession session);
    public interface ISubject
    {
        public event NotifyError NotifyError;
        public event NotifyInfo NotifyInfo;
        public event NotifyException NotifyException;
        public event LoggingInfo LoggingInfo;
        public event NotifyShutdown NotifyShutdown;
    }
}
