using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace TeslaCommon
{
    [Serializable]
    public class UserData : IMemberData
    {
        public string UID { get; set; }
        public string Name { get; set; }
        public UserData(string memberName)
        {
            Name = memberName;
            UID = Guid.NewGuid().ToString();
        }
    }
}
