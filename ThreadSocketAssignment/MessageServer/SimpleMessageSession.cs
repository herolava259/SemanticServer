using Common;
using Common.SimpleMessageProtocol;
using Common.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Common.Exceptions;

namespace MessageServer
{
    public class SimpleMessageSession : IDisposable, ISubject
    {
        public event NotifyError NotifyError = (s, e) => Console.WriteLine("Init NotifyError Observer");
        public event NotifyInfo NotifyInfo;
        public event NotifyException NotifyException;
        public event LoggingInfo LoggingInfo;
        public event NotifyShutdown NotifyShutdown;

        public readonly Guid Id;
        public SimpleMessageSession(Socket conn, int ThreadId)
        {
            Id = Guid.NewGuid();
            _conn = conn;
            _threadId = ThreadId;

            NotifyError = (s, e) => Console.WriteLine("Start Notify Error");
            NotifyInfo = (s, t, i) => Console.WriteLine("Start Notify Information");

            NotifyException = (s, ex) => Console.WriteLine("Start Notify Exception");

            LoggingInfo = (s, m) => Console.WriteLine("Start Logging Info");

            NotifyShutdown = s => Console.WriteLine("Start Notify Shutdown");
        }
        public int _threadId { get; private set; }
        private Socket _conn;

        public SimpleMessageSession(Socket conn)
        {
            Id = Guid.NewGuid();
            _conn = conn;

            NotifyError = (s, e) => Console.WriteLine("Start Notify Error");
            NotifyInfo = (s, t, i) => Console.WriteLine("Start Notify Information");

            NotifyException = (s, ex) => Console.WriteLine("Start Notify Exception");

            LoggingInfo = (s, m) => Console.WriteLine("Start Logging Info");

            NotifyShutdown = s => Console.WriteLine("Start Notify Shutdown");
        }

        public SimpleMessageSession()
        {
            Id = Guid.NewGuid();
            //_conn = default;

            NotifyError = (s, e) => Console.WriteLine("Start Notify Error");
            NotifyInfo = (s, t, i) => Console.WriteLine("Start Notify Information");

            NotifyException = (s, ex) => Console.WriteLine("Start Notify Exception");

            LoggingInfo = (s, m) => Console.WriteLine("Start Logging Info");

            NotifyShutdown = s => Console.WriteLine("Start Notify Shutdown");
        }
        

        public void SetThreadId(int id)
        {
            _threadId = id;
        }

        public void SetConnection(Socket conn)
        {
            _conn = conn;
        }

        public void Clear()
        {
            Dispose();
            _threadId = -1;
            
        }
        

        public void Dispose()
        {
            _conn?.Shutdown(SocketShutdown.Both);
            _conn?.Close();

        }

        private int Send(string msg)
        {
            byte[] data = Encoding.ASCII.GetBytes(msg);

            int byteSent = _conn.Send(data);

            return byteSent;

        }

        private bool SendingResponse(ProtocolConstant.SimpleResponseCode status,string bodyMsg = "")
        {
            try
            {
                ProtocolPackage package = new ProtocolPackage();

                package.SetHeader(new ProtocolHeader
                {
                    ProtoType = ProtocolConstant.ResponseType,
                    StatusCode = (int)status
                });

                if (!String.IsNullOrEmpty(bodyMsg))
                {
                    package.GetBody().Content = bodyMsg;
                }


                int byteSend = Send(package.GetCompletedPackageString());

                if (byteSend > 0)
                {
                    this.LoggingInfo?.Invoke(this, $"Sending a package response with Status Code: {status} has size is {byteSend} bytes");
                
                }
                else
                {
                    this.NotifyError?.Invoke(this, $"Sending response package is failed!");
                    return false;
                }   

            }
            catch (Exception ex)
            {
                this.NotifyException?.Invoke(this, ex);
                return false;
            }
            return true;
        }

        private int Receive(out string msgRec)
        {
            byte[] buffer = new byte[2048];

            int bytesRec = 0;

            msgRec = "";

            bytesRec = _conn.Receive(buffer);

            if (bytesRec < 0)
            {
                this.LoggingInfo?.Invoke(this,"Receiving message from Client is failed!");
                
            }
            else
            {
                msgRec += Encoding.UTF8.GetString(buffer, 0, bytesRec);
            }
            return bytesRec;
        }

        private ProtocolConstant.SimpleResponseCode ProcessingRequest(string msgRec)
        {
            if (msgRec == null)
                return ProtocolConstant.SimpleResponseCode.ErrorRequest;

            
            ProtocolPackage package = ProtocolParser.ParseToPackage(msgRec);

            if(package.GetHeader().ProtoType != ProtocolConstant.RequestType)
            {
                this.NotifyError?.Invoke(this, "Request package type is wrong!");
                
                return ProtocolConstant.SimpleResponseCode.ErrorRequest;
            }
            ProtocolConstant.SimpleResponseCode statusResp;
            string respBody = "";
            switch((ProtocolConstant.SimpleRequestCode)package.GetHeader().StatusCode) 
            {
                case ProtocolConstant.SimpleRequestCode.ReqConnect:
                    {
                        statusResp = ProtocolConstant.SimpleResponseCode.SuccessConnect;
                        break;
                    }
                case ProtocolConstant.SimpleRequestCode.ReqSendMSG:
                    {
                        statusResp = ProtocolConstant.SimpleResponseCode.SuccesSendMSG;

                        Utility.WriteMessageToAuditFile(ProtocolConstant.AuditPathFile,
                                                        (package?.GetBody() as RequestBody)?.Message, _threadId);


                        break;
                    }
                case ProtocolConstant.SimpleRequestCode.ReqQuitSession:
                    {
                        statusResp = ProtocolConstant.SimpleResponseCode.SuccesQuitSession;

                        break;
                    }
                default:
                    {
                        throw new ProtocolException($"Can't Have feature with requset code{package.GetHeader().StatusCode}");
                    }
            }

            if(!SendingResponse(statusResp, respBody))
            {
                throw new ProtocolException("Error While sending response");
            }


            return statusResp;
        }

        public void Serving()
        {
            string msgRec ="";
            int bytesRec = 0;
            try
            {
                while (true)
                {
                    bytesRec = Receive(out msgRec);
                    if (bytesRec < 0)
                    {
                        this.NotifyError?.Invoke(this, "Error while receive message from client");
                        this.NotifyError?.Invoke(this, "Stop Serving");
                        
                        return;
                    }
                    else
                    {
                        this.LoggingInfo?.Invoke(this, $"Receive 1 package from client with {bytesRec} byte data");
                        this.LoggingInfo?.Invoke(this, msgRec);
                        
                    }

                    var respCode = ProcessingRequest(msgRec);

                    if((int)respCode > 30 || respCode == ProtocolConstant.SimpleResponseCode.SuccesQuitSession)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                this.NotifyException?.Invoke(this, ex);
            }


        }

        public void Shutdown()
        {
            Clear();
            this.NotifyShutdown?.Invoke(this);
        }
    }
}
