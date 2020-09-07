using System;

namespace TeslaCommon
{
    public interface IPacket
    {
        ClientData Source { get; set; }
        ClientData Destination { get; set;}
        DateTime MessageTime { get; set; }
    }
}
