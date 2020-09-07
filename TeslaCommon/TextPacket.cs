using System;

namespace TeslaCommon
{
    [Serializable]
    public class TextPacket : IPacket
    {
        public ClientData Source { get; set; }
        public ClientData Destination { get; set; }
        public DateTime MessageTime { get; set; }
        public string Message { get; set; }
        public TextPacket(string msg, ClientData src, ClientData dst)
        {
            Source = src;
            Destination = dst;
            Message = msg;
            MessageTime = DateTime.Now;
        }
    }
}
