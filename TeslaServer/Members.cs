using System.Collections.Generic;
using TeslaCommon;

namespace TeslaServer
{
    public class Members
    {
        public Dictionary<string, IMember> TeslaMembers { get; set; }
        public Members()
        {
            TeslaMembers = new Dictionary<string, IMember>();
        }
        public void AddMember(IMember member)
        {
            if (!TeslaMembers.ContainsKey(member.Name))
            {
                TeslaMembers.Add(member.Name, member);
                return;
            }
        }
        public void RemoveMember(string memberName)
        {
            if (TeslaMembers.ContainsKey(memberName))
            {
                TeslaMembers.Remove(memberName);
                return;
            }
        }

    }
}
