using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Exceptions
{
    public class ProtocolException : Exception
    {
        public ProtocolException() : base() { }

        public ProtocolException(string msg) : base($"Simple Protocol Exception: {msg}") { }

        public ProtocolException(string format, params object[] objects) : base(String.Format(format,objects)) { }

        public ProtocolException(string msg, Exception ex) : base(msg, ex) { }

        
    }
}
