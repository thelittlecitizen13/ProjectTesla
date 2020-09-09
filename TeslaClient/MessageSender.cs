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
            if (_inputManager.IsSendPicture(msg))
            {
                if (_inputManager.IsSendScreenShot(msg))
                    SendNewImageMessage((UserData)sourceMember, (UserData)destinationMember);
                else
                {
                    string imgPath = System.Text.RegularExpressions.Regex.Split(msg, ";")[1];
                    if (_inputManager.IsFileExists(imgPath))
                        SendNewImageMessage(imgPath, (UserData)sourceMember, (UserData)destinationMember);
                    else
                        _outputManager.DisplayText("Error - img not found");
                }
            }
            else
            {
                SendNewTextMessage(msg, (UserData)sourceMember, (UserData)destinationMember);
            }

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
        public void SendNewImageMessage(UserData src, UserData dst)
        {
            IMessage message = new ImageMessage(takeScreenShot(), src, dst);
            _binaryFormatter.Serialize(_nwStream, message);
        }
        public void SendNewImageMessage(string imagePath, UserData src, UserData dst)
        {
            var img = loadImage(imagePath);
            if (img == null)
            {
                SendNewTextMessage("Tried to send a message with no success", src, dst);
                return;
            }
            IMessage message = new ImageMessage(img, src, dst);
            _binaryFormatter.Serialize(_nwStream, message);
        }
        private Bitmap takeScreenShot()
        {
            var bitmap = new Bitmap(1920, 1080);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(0, 0, 0, 0,
                bitmap.Size, CopyPixelOperation.SourceCopy);
            }
            CreateImageFolder();
            bitmap.Save(@"C:\images\clientPrintScreen" + Guid.NewGuid() + ".jpg", ImageFormat.Jpeg);
            return bitmap;
        }
        private Bitmap loadImage(string path)
        {
            try
            {
                return new Bitmap(path);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }
        private void CreateImageFolder()
        {
            System.IO.Directory.CreateDirectory(@"C:\images");
        }
        public void HandleUserCommands(string msg)
        {
            string[] userArgs = msg.Split(" ");
            IMessage messageFromCommand = _commandManager.GenerateMessageFromCommand(userArgs);
            if (messageFromCommand != null)
                SendNewMessage(messageFromCommand);
            else
                _outputManager.DisplayText(_commandManager.GetCommandHelp());

        }
    }
}
