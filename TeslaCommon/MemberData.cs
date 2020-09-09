using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace TeslaCommon
{
    [Serializable]
    public class MemberData : IMemberData
    {
        public string UID { get; set; }
        public string Name { get; set; }
        public MemberData(string memberName)
        {
            Name = memberName;
            UID = Guid.NewGuid().ToString();
        }
    }
}
