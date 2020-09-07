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
            string textToShow = $"{msg.MessageTime.ToString()} - {msg.Source.ClientName}: {msg.Message}";
            _outputManager.DisplayText(textToShow);
        }

        private void processImageMessage(ImageMessage msg)
        {
            string textToShow = $"{msg.MessageTime.ToString()} - {msg.Source.ClientName}: Image Received";
            _outputManager.DisplayText(textToShow);
            string imgPath = _outputManager.SaveAnImage(msg.Image);
            _outputManager.DisplayAnImage(imgPath);
        }

        public IMessage ReceiveAMessage()
        {
            var dataReceived = _binaryFormatter.Deserialize(_nwStream);
            System.Console.WriteLine(dataReceived.GetType()); // Debugging!
            IMessage messageReceived;
            try
            {
                // ToDo: Instead of try catch, try to GetType() on dataReceived and check of equals to relvant types
                messageReceived = (TextMessage)dataReceived;
                return messageReceived;
            }
            catch { }
            try
            {
                messageReceived = (ImageMessage)dataReceived;
                return messageReceived;
            }
            catch { }
            return null;
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
