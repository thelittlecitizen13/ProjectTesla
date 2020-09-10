using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using TeslaCommon;

namespace TeslaClient
{
    public class MessageSender
    {
        private ISerializer _serializer;
        private ClientData _clientData;
        private string _name;
        public MessageSender(ClientData clientData ,string name)
        {
            _clientData = clientData;
            _serializer = new BinarySerializer();
            _name = name;
        }
        public void SendNewMessage(string msg, IMemberData destinationMember, IMemberData sourceMember, UserData messageAuthor)
        {
            if (destinationMember.GetType() == typeof(GroupData))
            {
                SendNewGroupMessage(msg, messageAuthor, (GroupData)sourceMember, (GroupData)destinationMember);
                return;
            }
            SendNewTextMessage(msg, (UserData)sourceMember, (UserData)destinationMember);
        }
        public void SendNewMessage(IMessage message)
        {
            _serializer.Serialize(_clientData.NwStream, message);
        }
        public void SendNewTextMessage(string text, UserData src, UserData dst)
        {
            IMessage message = new TextMessage(text, src, dst);
            _serializer.Serialize(_clientData.NwStream, message);
        }
        public void SendNewGroupMessage(string text, UserData author, GroupData src, GroupData dst)
        {
            IMessage message = new GroupMessage(text, author, src, dst);
            _serializer.Serialize(_clientData.NwStream, message);
        }

        public void HandleUserCommands(string msg, IMemberData source, IMemberData destination)
        {
            string[] userArgs = msg.Split(" ");
            IMessage messageFromCommand = _clientData.commandManager.GenerateMessageFromCommand(userArgs, source, destination);
            if (messageFromCommand != null)
                SendNewMessage(messageFromCommand);
            else
            {
                _clientData.Outputter.DisplayText(_clientData.commandManager.GetCommandHelp());
                _clientData.Outputter.DisplayText("Press any key to continue..", ConsoleColor.Gray);
                _clientData.Inputter.ReadLine();
            }

        }
        
    }
}
