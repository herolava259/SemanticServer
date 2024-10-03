using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Common.Exceptions;
using Common;
using Common.SimpleMessageProtocol;
using Common.Utilities;
using System.Security.AccessControl;
using Common.CommunicationModel;
using Common.Extensions;

namespace MessageClient
{
    public class SimpleMessageClient : IDisposable
    {
        
        private string _domainName;
        private int _port;
        private Socket _sock;
        private IPEndPoint _endPoint;

        private readonly IStateBucket _stateBucket;

        public SimpleMessageClient(IStateBucket bucket,string domainName = "localhost", int port = 11000)
        {
            _stateBucket = bucket;
            _domainName = domainName;
            _port = port;

        }

        public void Dispose()
        {
            if (_sock != null)
            {
                _sock.Shutdown(SocketShutdown.Both);
                _sock.Close();
            }
            
        }

        public void CloseConnection()
        {
            if(_sock != null)
            {
                _sock.Shutdown(SocketShutdown.Both);
                _sock.Close();
            }
        }

        public bool Init(SocketType sockType = SocketType.Stream, ProtocolType protoType = ProtocolType.Tcp)
        {
            
            try
            {
                _stateBucket.Logging("Start initting socket!");
                var host = Dns.GetHostEntry(_domainName);
                var ipAddr = host.AddressList[0];
                _endPoint = new IPEndPoint(ipAddr, _port);
                _sock = new Socket(ipAddr.AddressFamily,sockType, protoType);

            }
            catch(SocketException se)
            {
                _stateBucket.AddExceptionError(se);
                return false;
            }
            catch(Exception ex)
            {
                _stateBucket.AddExceptionError(ex);
                return false;
            }
            _stateBucket.Logging("Initting Socket is sucessfully!");

            return true;
        }

        public async Task ConnectAsync()
        {
            try
            {
                await _sock.ConnectAsync(_endPoint);
            }catch(Exception ex)
            {
                throw new ProtocolException(ex.Message);
            }
        }

        private int Send(string msg)
        {
            byte[] data = Encoding.ASCII.GetBytes(msg);

            int byteSent = _sock.Send(data);
            return byteSent;
        }

        private int Receive(out string msg)
        {
            var buffer = new byte[2048];
            int byteRec = _sock.Receive(buffer);
            msg = "";
            if(byteRec < 0) {
                _stateBucket.AddError("Receiving message from server is failed!");
            }
            else
            {
               msg = Encoding.ASCII.GetString(buffer,0,byteRec);
            }
            return byteRec;
        }

        public bool RequestConnect()
        {
            try
            {
                ProtocolPackage package = new ProtocolPackage();

                package.SetHeader(new ProtocolHeader
                {
                    ProtoType = ProtocolConstant.RequestType,
                    StatusCode = (int)ProtocolConstant.SimpleRequestCode.ReqConnect
                });

                int byteSend = Send(package.GetCompletedPackageString());

                if(byteSend > 0)
                {
                    _stateBucket.Logging($"Sending a package with connect request has size is {byteSend} byte");
                    
                }
                else
                {
                    _stateBucket.AddError($"Sending request package failed!");
                    return false;
                }

                string msgRec = "";
                int byteRec = Receive(out msgRec);

                if(byteRec < 0)
                {
                    _stateBucket.AddError("Receiving response from server failed");
                    return false;
                }

                _stateBucket.Logging($"Receiving response with {byteRec} byte");
                 package = ProtocolParser.ParseToPackage(msgRec);

                 if(package.GetHeader().ProtoType == ProtocolConstant.ResponseType) {
                    bool check = package.GetHeader().StatusCode == (int)ProtocolConstant.SimpleResponseCode.SuccessConnect;

                    if(!check)
                    {
                        _stateBucket.AddError("Server rejected connect request from you");
                        return false;
                    }

                    _stateBucket.Logging("Connecting request is success");
                }

            }
            catch(Exception ex)
            {
                _stateBucket.AddExceptionError(ex);
                return false;
            }
            return true;
        }


        public bool RequestSendMessage(MessageDTO message)
        {

            try
            {
                ProtocolPackage pkg = new ProtocolPackage();

                pkg.AddHeader("Type", ProtocolConstant.RequestType)
                   .AddHeader("Status Code", ProtocolConstant.SimpleRequestCode.ReqSendMSG);

                pkg.SetBody(new RequestBody()
                {
                    Message = message.ToMessage()
                });

                int byteSend = Send(pkg.GetCompletedPackageString());

                if(byteSend < 0)
                {
                    _stateBucket.AddError("Sending message to server is failed");
                    return false;
                }

                _stateBucket.Logging($"Sending message to server successfully with {byteSend} byte data.");

                string msgRec = "";

                int byteRec = Receive(out msgRec);

                if(byteRec < 0) {

                    _stateBucket.AddError("Receiving response from server is failed!");
                    return false;
                }

                var respPkg = ProtocolParser.ParseToPackage(msgRec);
                
                if(respPkg.GetHeader().ProtoType != ProtocolConstant.ResponseType)
                {
                    _stateBucket.AddError("Response with type from server is invalid!");
                    return false;
                }

                

                if(respPkg.GetHeader().StatusCode == (int)ProtocolConstant.SimpleResponseCode.SuccesSendMSG) {

                    _stateBucket.Logging("Server accepted to message from you!");
                    return true;
                }
                else
                {
                    _stateBucket.AddError("Ohh! Server does not accepted from you!");
                    return false;
                }

            }
            catch(Exception ex)
            {
                _stateBucket.AddExceptionError(ex);
                return false;
            }
            return true;
        }

        public bool RequestQuitSession()
        {
            try
            {
                ProtocolPackage pkg = new ProtocolPackage();

                pkg.AddHeader("Type", ProtocolConstant.RequestType)
                   .AddHeader("Status Code", ProtocolConstant.SimpleRequestCode.ReqQuitSession);


                int byteSend = Send(pkg.GetCompletedPackageString());

                if (byteSend < 0)
                {
                    _stateBucket.AddError("Request quitting session to server is failed");
                    return false;
                }

                _stateBucket.Logging($"Request quitting session server successfully with {byteSend} byte data.");

                string msgRec = "";

                int byteRec = Receive(out msgRec);

                if (byteRec < 0)
                {

                    _stateBucket.AddError("Request response from server is failed!");
                    return false;
                }

                var respPkg = ProtocolParser.ParseToPackage(msgRec);

                if (respPkg.GetHeader().ProtoType != ProtocolConstant.ResponseType)
                {
                    _stateBucket.AddError("Response with type from server is invalid!");
                    return false;
                }



                if (respPkg.GetHeader().StatusCode == (int)ProtocolConstant.SimpleResponseCode.SuccesQuitSession)
                {

                    _stateBucket.Logging("Server accepted to shutdown session from you!");
                    return true;
                }
                else
                {
                    _stateBucket.AddError("Ohh! Server does not accepted from you!");
                    return false;
                }

            }
            catch (Exception ex)
            {
                _stateBucket.AddExceptionError(ex);
                return false;
            }
            
        }

        public IStateBucket GetLoggingBucket()
        {
            return _stateBucket;
        }
    }
}
