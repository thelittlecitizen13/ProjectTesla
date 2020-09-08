using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using TeslaCommon;

namespace TeslaClient
{
    public class MessageReceiver
    {
        private NetworkStream _nwStream;
        private IFormatter _binaryFormatter;
        private OutputManager _outputManager;
        
        public MessageReceiver(NetworkStream networkStream, OutputManager outputManager)
        {
            _nwStream = networkStream;
            _binaryFormatter = new BinaryFormatter();
            _outputManager = outputManager;
        }

        private void processMessage(IMessage msg)
        {
            if (msg == null)
            {
                _outputManager.DisplayText("Fatal - Message display failed"); // for Debug
            }
            if (msg is TextMessage)
            {
                processTextMessage((TextMessage)msg);
            }
            else
            {
                if (msg is ImageMessage)
                {
                    processImageMessage((ImageMessage)msg);                    
                }
                else
                {
                    _outputManager.DisplayText("Fatal - Message display failed"); // for Debug
                }
            }
        }
        private void processTextMessage(TextMessage msg)
        {
            string textToShow = $"{msg.MessageTime.ToString()} - {msg.Source.MemberName}: {msg.Message}";
            _outputManager.DisplayText(textToShow);
        }

        private void processImageMessage(ImageMessage msg)
        {
            string textToShow = $"{msg.MessageTime.ToString()} - {msg.Source.MemberName}: Image Received";
            _outputManager.DisplayText(textToShow);
            string imgPath = _outputManager.SaveAnImage(msg.Image);
            _outputManager.DisplayAnImage(imgPath);
        }

        public IMessage ReceiveAMessage()
        {
            var dataReceived = _binaryFormatter.Deserialize(_nwStream);
            return (IMessage)dataReceived;
        }
        
        public void Run()
        {
            while (true)
            {
                IMessage msg = ReceiveAMessage();
                processMessage(msg);
            }
        }

    }
}
