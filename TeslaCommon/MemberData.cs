using System;
using System.Collections.Generic;
using System.Text;

namespace TeslaCommon
{
    [Serializable]
    public class MemberData
    {
        public string UID { get; set; }
        public string ClientName { get; set; }
        public MemberData(string clientName)
        {
            ClientName = clientName;
            UID = Guid.NewGuid().ToString();
        }
    }
}
