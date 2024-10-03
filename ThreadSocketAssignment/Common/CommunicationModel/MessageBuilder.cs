using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.CommunicationModel
{
    public class MessageBuilder : IMessageBuilder
    {

        private string Content = "Default";
        private DateTime SendingTime = DateTime.Now;

        private string UserName = "Farrer";

        private string EmailAddress = "";

        private string Title = "Message from client";

        private static MessageBuilder _instance;

        private MessageBuilder()
        {

            SendingTime = DateTime.Now;
        }

        public static MessageBuilder GetInstance()
        {
            if(_instance == null )
            {
                _instance = new MessageBuilder();
            }

            return _instance;
        }

        public MessageBuilder WithTitle(string title)
        {
            Title = title;
            return this;
        }

        public MessageBuilder WithUserName(string name)
        {
            UserName = name;
            return this;
        }

        public MessageBuilder WithEmail(string email)
        {
            EmailAddress = email;
            return this;
        }

        public MessageBuilder WithSendingTime(bool isNow = true)
        {
            if (isNow)
            {
                SendingTime = DateTime.Now;
            }
            else
            {
                SendingTime = DateTime.MinValue;
            }

            return this;
        }

        public MessageBuilder WithSendingTime(DateTime time)
        {
            SendingTime = time;
            return this;
        }

        public MessageBuilder WithContent(string content)
        {
            Content = content;
            return this;
        }
        public Message Build()
        {
            return new Message()
            {
                Title = Title,
                UserName = UserName,
                EmailAddress = EmailAddress,
                SendingTime = SendingTime,
                Content = Content,
            };
        }
    }
}
