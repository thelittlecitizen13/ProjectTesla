using System.Net.Sockets;

namespace TeslaClient
{
    public class MessageSender
    {
        private NetworkStream nwStream;
        public MessageSender(NetworkStream networkStream)
        {
            nwStream = networkStream;
        }
        public void SendNewMessage()
        {

        }
    }
}
