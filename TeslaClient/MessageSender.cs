using System.Drawing;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using TeslaCommon;

namespace TeslaClient
{
    public class MessageSender
    {
        private NetworkStream nwStream;
        private IFormatter _binaryFormatter;
        public MessageSender(NetworkStream networkStream)
        {
            nwStream = networkStream;
            _binaryFormatter = new BinaryFormatter();
        }
        public void SendNewMessage()
        {

        }
        public void SendNewTextMessage(string text, ClientData src, ClientData dst)
        {
            // ToDo: change the SRC and DST to real 
            TextMessage message = new TextMessage(text, src, dst);

            byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(Text);
            nwStream.Write(bytesToSend, 0, bytesToSend.Length);
        }
        public void SendNewImageMessage(Bitmap img)
        {

        }
        
    }
}
