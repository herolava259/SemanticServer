using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Utilities
{
    public static class ProtocolConstant
    {
        public const string BeginOfPackage = "\n<Package>";

        public const string EndOfPackage = "</Package>\r\n";

        public const string BeginOfHeader = "\n<Header>";

        public const string EndOfHeader = "</Header>\n";

        public const string BeginOfBody = "\n<Body>";

        public const string EndOfBody = "</Body>\n";

        public const string BeginOfRowHeader = "\n<RowHeader>";


        public const string EndOfRowHeader = "</RowHeader>\n";

        public const string RequestType = "<Req>";

        public const string ResponseType = "<Resp>";

        public const string PackagePattern = @"^\n<Package>([\s\S]*)</Package>\r\n$";

        public const string HeaderPattern = @"\n<Header>\n<RowHeader>(<Req>|<Resp>)</RowHeader>\n\n<RowHeader>(\d+)</RowHeader>\n</Header>\n";

        public const string BodyPattern = @"\n<Body>([\s\S]*)</Body>\n";

        public const string RowHeaderPattern = @"\n<RowHeader>([\s\S]*)</RowHeader>\n";

        public const string EmailPattern = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$";


        public const string AuditPathFile = "./audit.xml";

        public enum SimpleRequestCode : int
        {
            ReqConnect = 10,
            ReqSendMSG = 11,
            ReqDisconnect = 13,
            ReqQuitSession = 12,
            ReqHistories = 14,
        }

        public enum SimpleResponseCode : int
        {
            SuccessRequest = 20,
            SuccessConnect = 21,
            SuccesSendMSG = 22,
            SuccesDisconnect = 23,
            SuccesQuitSession = 24,

            ErrorRequest = 30,
            ErrorConnect = 31,
            ErrorSendMSG = 32,
            ErrorDisconnect = 33,
            ErrorQuitSession = 34,
        }
    }
}
