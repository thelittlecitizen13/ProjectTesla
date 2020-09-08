using System;
using System.Collections.Generic;
using System.Text;

namespace TeslaCommon
{
    [Serializable]
    public class MemberData
    {
        public string UID { get; set; }
        public string MemberName { get; set; }
        public MemberData(string memberName)
        {
            MemberName = memberName;
            UID = Guid.NewGuid().ToString();
        }
    }
}
