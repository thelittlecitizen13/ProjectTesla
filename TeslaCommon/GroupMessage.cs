using System;

namespace TeslaCommon
{
    [Serializable]
    public class GroupMessage : IMessage
    {
        public IMemberData Source { get; set; }
        public IMemberData Destination { get; set; }
        public DateTime MessageTime { get; set; }
        public GroupData GroupChanged { get; set; }
        public ChangeType typeOfChange { get; set; }
        public GroupMessage(GroupData group, ChangeType action, UserData src, UserData dst)
        {
            GroupChanged = group;
            typeOfChange = action;
            Source = src;
            Destination = dst;
            MessageTime = DateTime.Now;
        }
    }
}
