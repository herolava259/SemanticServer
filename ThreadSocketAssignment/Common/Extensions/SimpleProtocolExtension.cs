using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.CommunicationModel;
namespace Common.Extensions
{
    public static class SimpleProtocolExtension
    {
        public static Message ToMessage(this MessageDTO dto) { 

            return MessageBuilder.GetInstance().WithTitle(dto.Title)
                                               .WithContent(dto.Content)
                                               .WithEmail(dto.EmailAddress)
                                               .WithUserName(dto.UserName)
                                               .WithSendingTime(isNow: true)
                                               .Build();

        }

        public static MessageDTO ToMessageDTO(this Message message) {

            return new MessageDTO()
            {
                Title = message.Title,
                Content = message.Content,
                EmailAddress = message.EmailAddress,
                UserName = message.UserName,
            };
        }
    }
}
