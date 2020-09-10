using System;

namespace TeslaCommon
{
    public class CommandMessage : IMessage
    {
        public IMemberData Source { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IMemberData Destination { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime MessageTime { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
