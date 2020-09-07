using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using TeslaCommon;

namespace TeslaClient
{
    public class MessageReceiver
    {
        private NetworkStream _nwStream;
        private IFormatter _binaryFormatter;
        public MessageReceiver(NetworkStream networkStream)
        {
            _nwStream = networkStream;
            _binaryFormatter = new BinaryFormatter();
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
        private void processMessage(IMessage msg)
        {
            
        }
        public void Run()
        {
            
        }

    }
}
