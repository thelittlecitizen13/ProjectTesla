using System;

namespace TeslaCommon
{
    [Serializable]
    public class TextMessage : IMessage
    {
        public IMemberData Source { get; set; }
        public IMemberData Destination { get; set; }
        public DateTime MessageTime { get; set; }
        public string Message { get; set; }
        public TextMessage(string msg, UserData src, UserData dst)
        {
            Source = src;
            Destination = dst;
            Message = msg;
            MessageTime = DateTime.Now;
        }
    }
}
