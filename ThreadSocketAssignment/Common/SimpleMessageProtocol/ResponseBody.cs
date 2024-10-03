using Common.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.SimpleMessageProtocol
{
    public class ResponseBody : AProtocolBody
    {
        private string _message = "";
        public ResponseBody(string msg = "") { 
            _message = msg;
        }
        public void AddMessage(string msg)
        {
            this._message +=  $"\n{msg}";
        }

        public void SetMessage(string msg)
        {
            _message = msg;
        }

        
        protected virtual string GenerateBodyString()
        {
            return $"{ProtocolConstant.BeginOfBody}{_message}{ProtocolConstant.EndOfBody}";
        }
        public override string ToString()
        {
            return GenerateBodyString();
        }
    }
}
