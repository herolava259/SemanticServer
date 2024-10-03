using Common.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.SimpleMessageProtocol
{
    public class ProtocolPackage
    {
        private AProtocolBody _body;
        private ProtocolHeader _header;

        public ProtocolPackage()
        {
            _body = new ResponseBody();
            _header = new ProtocolHeader();
        }

        public ProtocolPackage SetHeader(ProtocolHeader header)
        {
            _header = header;
            return this;
        }

        public ProtocolPackage SetBody(AProtocolBody body)
        {
            _body = body; 
            return this;
        }

        public ProtocolHeader GetHeader()
        {
            return _header;
        }

        public AProtocolBody GetBody()
        {
            return _body;
        }

        public ProtocolPackage AddHeader(string headerName,  object value)
        {
            if (headerName == null) throw new ArgumentNullException();

            if(headerName == "Type")
            {
                _header.ProtoType = value as string;
            }
            else if(headerName == "Status Code")
            {
                _header.StatusCode = (int)value;
            }
            else
            {
                throw new ArgumentException("Header Type Name is invalid!");
            }
            return this;
        }


        public string GetCompletedPackageString()
        {
            return $"{ProtocolConstant.BeginOfPackage}{_header.ToString()}{_body.ToString()}{ProtocolConstant.EndOfPackage}";

        }

        public byte[] ToBytes()
        {
            return Encoding.ASCII.GetBytes(GetCompletedPackageString());
        }
    }
}
