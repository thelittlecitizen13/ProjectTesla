using System;

namespace TeslaCommon
{
    public class CommandMessage : IMessage
    {
        public MemberData Source { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public MemberData Destination { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime MessageTime { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
