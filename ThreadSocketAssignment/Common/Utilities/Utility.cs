using Common.CommunicationModel;
using Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Linq;

namespace Common.Utilities
{
    public static class Utility
    {
        private static XElement root = new ("Root");
        private static XDocument doc;
        public static void WriteMessageToAuditFile(string path, Message msg, int threadId)
        {
            try
            {
                if (!File.Exists(path))
                {
                    //var _ = File.Create(path);
                   doc = new XDocument(root);
                 
                }
                else
                {
                    doc = XDocument.Load(path);
                    root=doc.Element("Root");
                }
                
                var msgXml = new XElement("Message");
                msgXml.Add(new XElement("ThreadId", threadId));
                msgXml.Add(new XElement("Title", msg.Title));
                msgXml.Add(new XElement("UserName", msg.UserName));
                msgXml.Add(new XElement("EmailAddress", msg.EmailAddress));
                msgXml.Add(new XElement("Content", msg.Content));
                msgXml.Add(new XElement("SendingTime", msg.SendingTime.ToString("dd/MM/yyyy hh:mm:ss tt")));

                root.Add(msgXml);

                doc.Save(path);
            }
            catch (Exception ex)
            {
                throw new ProtocolException("Error: Write msg to .xml file error", ex);
            }
        }
    }
}
