using System;
using System.Collections.Generic;
using System.Linq;

namespace TeslaCommon
{
    [Serializable]
    public class Group : IMember
    {
        public string Name { get; set; }
        public IMemberData Data { get; set; }
        public List<UserData> GroupUsers { get; set; }
        public List<UserData> GroupManagers { get; set; }
        

        public Group(UserData memberData)
        {
            Data = memberData;
            Name = memberData.Name;
            GroupUsers = new List<UserData>();
            GroupManagers = new List<UserData>();
        }
        public string AddMember(UserData member)
        {
            //ToDo : check if the member already in the group
            GroupUsers.Add(member);
            return $"{member.Name} added successfully to {Name} group";
        }
        public string RemoveMember(UserData memberToRemove, UserData executer)
        {
            bool isExecuterManager = GroupManagers.Any(member => member.UID == executer.UID);
            if (isExecuterManager)
            {
                var toRemove = GroupUsers.Where(member => member.UID == memberToRemove.UID).ToList().First();
                if (toRemove != null)
                {
                    GroupUsers.Remove(toRemove);
                    return $"{toRemove.Name} removed from group {Name}";
                }
                return $"User {toRemove.Name} is not in group {Name}";
            }
            return $"You are not a manager of {Name} group";
        }


    }
}
