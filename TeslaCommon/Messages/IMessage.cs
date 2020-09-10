using System;

namespace TeslaCommon
{
    public interface IMessage
    {
        IMemberData Source { get; set; }
        IMemberData Destination { get; set;}
        DateTime MessageTime { get; set; }
    }
}
