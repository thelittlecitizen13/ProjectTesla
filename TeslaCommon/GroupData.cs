using System;
using System.Collections.Generic;

namespace TeslaCommon
{
    [Serializable]
    public class GroupData : IMemberData
    {
        public string UID { get; set; }
        public string Name { get; set; }
        public List<MemberData> Users { get; set; }
        public GroupData(string groupName, MemberData groupCreator)
        {
            UID = Guid.NewGuid().ToString();
            Name = groupName;
            Users = new List<MemberData>() { groupCreator };
        }
    }
}
