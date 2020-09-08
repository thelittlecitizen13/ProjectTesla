using System;
using System.Collections.Generic;
using System.Linq;

namespace TeslaCommon
{
    [Serializable]
    public class Group : IMember
    {
        public string Name { get; set; }
        public MemberData Data { get; set; }
        public List<MemberData> GroupUsers { get; set; }
        public List<MemberData> GroupManagers { get; set; }
        public Group(MemberData memberData)
        {
            Data = memberData;
            Name = memberData.MemberName;
            GroupUsers = new List<MemberData>();
            GroupManagers = new List<MemberData>();
        }
        public string AddMember(MemberData member)
        {
            //ToDo : check if the member already in the group
            GroupUsers.Add(member);
            return $"{member.MemberName} added successfully to {Name} group";
        }
        public string RemoveMember(MemberData memberToRemove, MemberData executer)
        {
            bool isExecuterManager = GroupManagers.Any(member => member.UID == executer.UID);
            if (isExecuterManager)
            {
                var toRemove = GroupUsers.Where(member => member.UID == memberToRemove.UID).ToList().First();
                if (toRemove != null)
                {
                    GroupUsers.Remove(toRemove);
                    return $"{toRemove.MemberName} removed from group {Name}";
                }
                return $"User {toRemove.MemberName} is not in group {Name}";
            }
            return $"You are not a manager of {Name} group";
        }


    }
}
