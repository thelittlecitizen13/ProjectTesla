﻿using System;
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
        private OutputManager _outputManeger;
        private InputManager _inputManager;
        private string _name;
        public MessageSender(NetworkStream networkStream, OutputManager outputManager, InputManager inputManager, string name)
        {
            _nwStream = networkStream;
            _binaryFormatter = new BinaryFormatter();
            _outputManeger = outputManager;
            _inputManager = inputManager;
            _name = name;
        }
        public void SendNewMessage()
        {
            _outputManeger.DisplayText("Enter your message");
            string msg = _inputManager.GetUserInput();
            if (_inputManager.IsSendPicture(msg))
            {
                if (_inputManager.IsSendScreenShot(msg))
                    SendNewImageMessage(new ClientData(_name), new ClientData("all")); // ToDo: get real ClientData when sending to users privately
                else
                {
                    string imgPath = System.Text.RegularExpressions.Regex.Split(msg, ";")[1];
                    if (_inputManager.IsFileExists(imgPath))
                        SendNewImageMessage(imgPath, new ClientData(_name), new ClientData("all")); // ToDo: get real ClientData when sending to users privately
                    else
                        _outputManeger.DisplayText("Error - img not found");
                }
            }
            else
            {
                SendNewTextMessage(msg, new ClientData(_name), new ClientData("all"));
            }

        }
        public void SendNewTextMessage(string text, ClientData src, ClientData dst)
        {
            // ToDo: change the SRC and DST to real 
            IMessage message = new TextMessage(text, src, dst);
            _binaryFormatter.Serialize(_nwStream, message);
        }
        public void SendNewImageMessage(ClientData src, ClientData dst)
        {
            IMessage message = new ImageMessage(takeScreenShot(), src, dst);
            _binaryFormatter.Serialize(_nwStream, message);
        }
        public void SendNewImageMessage(string imagePath, ClientData src, ClientData dst)
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
        
    }
}
