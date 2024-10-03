using Common.CommunicationModel;
using Common.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.SimpleMessageProtocol
{
    public class RequestBody : AProtocolBody
    {

        public Message Message { get; set; }
        public RequestBody()
        {
            
        }

        public void SetMessage(Message msg)
        {
            this.Message = msg;

        }

        protected virtual string ConvertToBodyString()
        {
            string msgString = "";
            if (Message != null)
            {
                msgString = JsonConvert.SerializeObject(new MessageJSON
                {
                    Title = Message.Title,
                    UserName = Message.UserName,
                    SendingTime = Message.SendingTime.ToString("dd/MM/yyyy hh:mm:ss tt"),
                    EmailAddress = Message.EmailAddress,
                    Content = Message.Content,
                });
                msgString = $"{ProtocolConstant.BeginOfBody}{msgString}{ProtocolConstant.EndOfBody}";
            }
            return msgString ;
        }
        public override string ToString()
        {
            return ConvertToBodyString() ;
        }
    }
}
