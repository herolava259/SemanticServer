using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.CommunicationModel
{
    public class Message
    {
        private static int incrementer = 0;

        public Message()
        {
            Id = incrementer++;
        }
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime SendingTime { get; set; }

        public string UserName { get; set; }

        public string EmailAddress { get; set; }

        public string Title { get; set; }

    }
}
