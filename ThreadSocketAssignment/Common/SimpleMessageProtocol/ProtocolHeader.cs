using Common.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Common.SimpleMessageProtocol
{
    public class ProtocolHeader
    {
        public string ProtoType { get; set; }

        public int StatusCode { get; set; }


        protected virtual string GenerateHeaderString()
        {
            string row1 = $"{ProtocolConstant.BeginOfRowHeader}{ProtoType}{ProtocolConstant.EndOfRowHeader}";
            string row2 = $"{ProtocolConstant.BeginOfRowHeader}{StatusCode}{ProtocolConstant.EndOfRowHeader}";
            return $"{ProtocolConstant.BeginOfHeader}{row1}{row2}{ProtocolConstant.EndOfHeader}";
        }
        public override string ToString()
        {
            return GenerateHeaderString();
        }


    }
}
