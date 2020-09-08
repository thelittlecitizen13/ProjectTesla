using System;
using System.Collections.Generic;
using System.Text;

namespace TeslaCommon
{
    interface IMember
    {
        string Name { get; set; }
        MemberData Data { get; set; }
    }
}
