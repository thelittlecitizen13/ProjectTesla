using System.Drawing;
using System.Net.Sockets;
using System.Text;

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
        public void SendNewTextMessage(string Text)
        {
            byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(Text);
            nwStream.Write(bytesToSend, 0, bytesToSend.Length);
        }
        public void SendNewImageMessage(Bitmap img)
        {

        }
    }
}
