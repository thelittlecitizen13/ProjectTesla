﻿using System;

namespace TeslaCommon
{
    public interface IMessage
    {
        ClientData Source { get; set; }
        ClientData Destination { get; set;}
        DateTime MessageTime { get; set; }
    }
}