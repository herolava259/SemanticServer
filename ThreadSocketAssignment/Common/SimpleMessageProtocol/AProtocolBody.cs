using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Common.SimpleMessageProtocol
{
    public abstract class AProtocolBody
    {
        public string Content { get; set; }
        public abstract string ToString();


    }
}
