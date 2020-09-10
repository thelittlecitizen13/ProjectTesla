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
        private NetworkStream _nwStream;
        private IFormatter _binaryFormatter;
        private OutputManager _outputManager;
        private InputManager _inputManager;
        private CommandManager _commandManager;
        private string _name;
        public MessageSender(NetworkStream networkStream, OutputManager outputManager, InputManager inputManager, string name, CommandManager commandManager)
        {
            _nwStream = networkStream;
            _binaryFormatter = new BinaryFormatter();
            _outputManager = outputManager;
            _inputManager = inputManager;
            _name = name;
            _commandManager = commandManager;
        }
        public void SendNewMessage(string msg, IMemberData destinationMember, IMemberData sourceMember, UserData messageAuthor)
        {
            // Should refactor
            if (destinationMember.GetType() == typeof(GroupData))
            {
                SendNewGroupMessage(msg, messageAuthor, (GroupData)sourceMember, (GroupData)destinationMember);
                return;
            }
            SendNewTextMessage(msg, (UserData)sourceMember, (UserData)destinationMember);

            //if (_inputManager.IsSendPicture(msg))
            //{
            //    if (_inputManager.IsSendScreenShot(msg))
            //        SendNewImageMessage((UserData)sourceMember, (UserData)destinationMember);
            //    else
            //    {
            //        string imgPath = System.Text.RegularExpressions.Regex.Split(msg, ";")[1];
            //        if (_inputManager.IsFileExists(imgPath))
            //            SendNewImageMessage(imgPath, (UserData)sourceMember, (UserData)destinationMember);
            //        else
            //            _outputManager.DisplayText("Error - img not found");
            //    }
            //}
            //else
            //{
            //    SendNewTextMessage(msg, (UserData)sourceMember, (UserData)destinationMember);
            //}

        }
        public void SendNewMessage(IMessage message)
        {
            _binaryFormatter.Serialize(_nwStream, message);
        }
        public void SendNewTextMessage(string text, UserData src, UserData dst)
        {
            IMessage message = new TextMessage(text, src, dst);
            _binaryFormatter.Serialize(_nwStream, message);
        }
        public void SendNewGroupMessage(string text, UserData author, GroupData src, GroupData dst)
        {
            IMessage message = new GroupMessage(text, author, src, dst);
            _binaryFormatter.Serialize(_nwStream, message);
        }

        public void HandleUserCommands(string msg, IMemberData source, IMemberData destination)
        {
            string[] userArgs = msg.Split(" ");
            IMessage messageFromCommand = _commandManager.GenerateMessageFromCommand(userArgs, source, destination);
            if (messageFromCommand != null)
                SendNewMessage(messageFromCommand);
            else
            {
                _outputManager.DisplayText(_commandManager.GetCommandHelp());
                _outputManager.DisplayText("Press any key to continue..", ConsoleColor.Gray);
                _inputManager.ReadLine();
            }

        }
        
    }
}
