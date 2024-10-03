using Common.CommunicationModel;
using Common.Exceptions;
using Common.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Common.SimpleMessageProtocol
{
    public class MessageJSON
    {
        public string Title { get; set; }
        public string Content { get; set; }

        public string SendingTime { get; set; }

        public string UserName { get; set; }

        public string EmailAddress { get; set; }

        
    }
    public static class ProtocolParser
    {
        public enum ParseType : int
        {
            Package,
            Header,
            Body,
            RowHeader,
            Default,
        }

        private static readonly Regex pkgReg;
        private static readonly Regex headerReg;
        private static readonly Regex bodyReg;
        private static readonly Regex rowHeaderReg;

        static ProtocolParser()
        {
            pkgReg = new Regex(ProtocolConstant.PackagePattern, RegexOptions.Compiled);
            headerReg = new Regex(ProtocolConstant.HeaderPattern, RegexOptions.Compiled);
            bodyReg = new Regex(ProtocolConstant.BodyPattern, RegexOptions.Compiled);
            rowHeaderReg = new Regex(ProtocolConstant.RowHeaderPattern, RegexOptions.Compiled);
        }
        private static bool IsMatch(ParseType type, string msg)
        {
            bool check = false;
            switch(type)
            {
                case ParseType.Package:
                    {
                        check = pkgReg.IsMatch(msg);
                        break;
                    }
                case ParseType.Header:
                    {
                        check = headerReg.IsMatch(msg);
                        break;
                    }
                case ParseType.Body:
                    {
                        check = bodyReg.IsMatch(msg);
                        break;

                    }
                case ParseType.Default:
                    {
                        check = pkgReg.IsMatch(msg);
                        break;
                    }
                case ParseType.RowHeader:
                    {
                        check = rowHeaderReg.IsMatch(msg);
                        break;
                    }
                default:
                    {
                        check = false;
                        break;
                    }
            }

            return check;

        }


        private static IEnumerable<string> SplitToRowHeaders(string msg)
        {
            var rows = new List<string>();

            int closeTagLen = ProtocolConstant.EndOfRowHeader.Length;

            int startIdx = 0;

            while(startIdx != -1)
            {
                var nextIdx = msg.IndexOf(ProtocolConstant.EndOfRowHeader, startIdx);

                if(nextIdx != -1)
                {
                    rows.Add(msg.Substring(startIdx, nextIdx+closeTagLen-startIdx));
                    nextIdx += closeTagLen;
                }

                startIdx = nextIdx;

            }

            return rows.Count > 0 ? rows.AsEnumerable() : null;
        }

        private static IEnumerable<string> GetContentWithinPart(ParseType type, string msg)
        {
            MatchCollection matches = null;
            switch (type)
            {
                case ParseType.Package:
                    {
                        matches = pkgReg.Matches(msg);
                        break;
                    }
                case ParseType.Header:
                    {
                        matches = headerReg.Matches(msg);
                        break;
                    }
                case ParseType.Body:
                    {
                        matches = bodyReg.Matches(msg);
                        break;

                    }
                case ParseType.Default:
                    {
                        matches = pkgReg.Matches(msg);
                        break;
                    }
                case ParseType.RowHeader:
                    {
                        matches = rowHeaderReg.Matches(msg);
                        return SplitToRowHeaders(matches.FirstOrDefault().Value);
                    }
                default:
                    {
                        matches = null;

                        break;
                    }
            }

            if(matches != null)
            {
                return matches.Select(x => x.Value);
            }
            else
            {
                return null;
            }
        }

        public static string GetRowContentFromHeader(string c)
        {
            int lengthRowTag = ProtocolConstant.BeginOfRowHeader.Length;
            int lengthEndRowTag = ProtocolConstant.EndOfRowHeader.Length;
            
            return c.Substring(lengthRowTag, c.Length - lengthRowTag - lengthEndRowTag);
        }

        public static string GetBodyContentFromBody(string c)
        {
            int lengthBodyTag = ProtocolConstant.BeginOfBody.Length;
            int lengthEndBodyTag = ProtocolConstant.EndOfBody.Length;

            return c.Substring(lengthBodyTag, c.Length - lengthBodyTag - lengthEndBodyTag);
        }

        public static Message ParseToRequestMessage(string content)
        {
            if (content == null)
                return null;
            var builder = MessageBuilder.GetInstance();


            try
            {
                var obj = JsonConvert.DeserializeObject<MessageJSON>(content);
                
                builder.WithTitle(obj.Title)
                       .WithUserName(obj.UserName)
                       .WithEmail(obj.EmailAddress)
                       .WithSendingTime(DateTime.ParseExact(obj.SendingTime, "dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture))
                       .WithContent(obj.Content);
            }
            catch (Exception e)
            {
                throw new ProtocolException("Protocol Exception: Error in ParseToRequestMessage()",e);
            }

            return builder.Build();
        }
        public static ProtocolPackage ParseToPackage(string msg)
        {
            ProtocolPackage pkg = new ProtocolPackage();

            if (IsMatch(ParseType.Package, msg))
            {
                var headerPart = GetContentWithinPart(ParseType.Header, msg);
                var bodyPart = GetContentWithinPart(ParseType.Body, msg);

                if (headerPart == null || bodyPart == null
                   || headerPart.Count() != 1 || bodyPart.Count() != 1)
                {
                    throw new ArgumentException("Message has not suitable format");
                }
                else
                {

                    var headerRows = GetContentWithinPart(ParseType.RowHeader, headerPart.FirstOrDefault());

                    var headerRowContents = headerRows.Select(c => GetRowContentFromHeader(c));//.ToList();
                    var bodyPartStr = bodyPart.FirstOrDefault();
                    var bodyContent = GetBodyContentFromBody(bodyPartStr);

                    var type = headerRowContents.Where(c => c == ProtocolConstant.RequestType || c == ProtocolConstant.ResponseType).FirstOrDefault();



                    if (type == null)
                        throw new ArgumentException("Protocol type is not valid");

                    var statusCode = Int32.Parse(headerRowContents.Where(c => c != type).FirstOrDefault());



                    if (type == ProtocolConstant.RequestType && statusCode == (int)ProtocolConstant.SimpleRequestCode.ReqSendMSG)
                    {
                        var reqBody = new RequestBody();
                        reqBody.SetMessage(ParseToRequestMessage(bodyContent));
                        pkg.SetBody(reqBody);
                    }
                    else
                    {
                        (pkg.GetBody() as ResponseBody).AddMessage(bodyContent);
                    }

                    pkg.SetHeader(new ProtocolHeader
                    {
                        ProtoType = type,
                        StatusCode = statusCode,
                    });
                }
            }
            else
            {
                throw new ArgumentException("Message is not format");
            }


            return pkg;
        }
        public static ProtocolPackage ParseToPackage(byte[] buffer, int begin, int end)
        {
            string msg = Encoding.UTF8.GetString(buffer, begin, end);
            
            return ParseToPackage(msg);
        }


    }
}
