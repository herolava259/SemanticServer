using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.CommunicationModel
{
    public class Audit
    {
        public int ThreadId { get; set; }
        public IEnumerable<string> ContentList { get; set; }

    }
}
