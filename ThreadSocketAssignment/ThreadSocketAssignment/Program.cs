using Common.SimpleMessageProtocol;
using Common.CommunicationModel;
using Common.Utilities;

namespace ThreadSocketAssignment
{
    public class Program
    {
        static void Main(string[] args)
        {
            // Test data package format
            // \n<Package>\n<Header>\n<RowHeader><Req></RowHeader>\n\n<RowHeader>11</RowHeader>\n</Header>\n</Package>\r\n
            var propkg = new ProtocolPackage();

            propkg.SetHeader(new ProtocolHeader
            {
                ProtoType = ProtocolConstant.RequestType,
                StatusCode = (int)ProtocolConstant.SimpleRequestCode.ReqSendMSG,
            });

            propkg.SetBody(
                new RequestBody()
                {
                    Message = new Message()
                    {
                        Title = "This is title",
                        Content = "This is message content",
                        UserName = "Farrer",
                        EmailAddress = "farrer.le@avepoint.com",
                        SendingTime = DateTime.Now,
                    }
                }
                );
            var pkg = ProtocolParser.ParseToPackage(propkg.GetCompletedPackageString());
            Console.WriteLine(pkg.GetCompletedPackageString());
        }
    }
}