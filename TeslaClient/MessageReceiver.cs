using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

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
    }
}
