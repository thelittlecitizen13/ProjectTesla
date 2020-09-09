using System;

namespace TeslaCommon
{
    [Serializable]
    public class GroupMessage : IMessage
    {
        public IMemberData Source { get; set; }
        public IMemberData Destination { get; set; }
        public UserData Author { get; set; }
        public DateTime MessageTime { get; set; }
        public string Message { get; set; }
        public GroupMessage(string msg, UserData author, GroupData src, GroupData dst)
        {
            Source = src;
            Destination = dst;
            Author = author;
            Message = msg;
            MessageTime = DateTime.Now;
        }
    }
}
