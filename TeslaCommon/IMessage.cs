using System;

namespace TeslaCommon
{
    public interface IMessage
    {
        MemberData Source { get; set; }
        MemberData Destination { get; set;}
        DateTime MessageTime { get; set; }
    }
}
