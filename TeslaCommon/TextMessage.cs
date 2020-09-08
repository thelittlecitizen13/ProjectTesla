using System;

namespace TeslaCommon
{
    [Serializable]
    public class TextMessage : IMessage
    {
        public MemberData Source { get; set; }
        public MemberData Destination { get; set; }
        public DateTime MessageTime { get; set; }
        public string Message { get; set; }
        public TextMessage(string msg, MemberData src, MemberData dst)
        {
            Source = src;
            Destination = dst;
            Message = msg;
            MessageTime = DateTime.Now;
        }
    }
}
