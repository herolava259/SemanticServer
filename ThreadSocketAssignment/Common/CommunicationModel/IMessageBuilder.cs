namespace Common.CommunicationModel
{
    public interface IMessageBuilder
    {
        Message Build();
        MessageBuilder WithContent(string content);
        MessageBuilder WithEmail(string email);
        MessageBuilder WithSendingTime(bool isNow = true);
        MessageBuilder WithSendingTime(DateTime time);
        MessageBuilder WithTitle(string title);
        MessageBuilder WithUserName(string name);
    }
}