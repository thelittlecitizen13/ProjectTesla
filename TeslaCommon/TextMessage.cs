using System;

namespace TeslaCommon
{
    [Serializable]
    public class TextMessage : IMessage
    {
        public ClientData Source { get; set; }
        public ClientData Destination { get; set; }
        public DateTime MessageTime { get; set; }
        public string Message { get; set; }
        public TextMessage(string msg, ClientData src, ClientData dst)
        {
            Source = src;
            Destination = dst;
            Message = msg;
            MessageTime = DateTime.Now;
        }
    }
}
