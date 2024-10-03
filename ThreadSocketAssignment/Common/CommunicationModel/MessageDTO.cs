using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.CommunicationModel
{
    public class MessageDTO
    {
        public string Content { get; set; }

        public string UserName { get; set; }

        public string EmailAddress { get; set; }

        public string Title { get; set; }
    }
}
