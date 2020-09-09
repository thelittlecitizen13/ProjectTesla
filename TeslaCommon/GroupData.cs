using System;
using System.Collections.Generic;

namespace TeslaCommon
{
    [Serializable]
    public class GroupData : IMemberData
    {
        public string UID { get; set; }
        public string Name { get; set; }
        public List<UserData> Users { get; set; }
        public List<UserData> GroupManagers { get; private set; }
        public GroupData(string groupName, UserData groupCreator)
        {
            UID = Guid.NewGuid().ToString();
            Name = groupName;
            Users = new List<UserData>() { groupCreator };
            GroupManagers = new List<UserData>() { groupCreator };
        }
        public GroupData(GroupData groupData)
        {
            UID = groupData.UID;
            Name = groupData.Name;
            Users = new List<UserData>(groupData.Users);
            GroupManagers = new List<UserData>(groupData.GroupManagers);
        }
        public void RemoveUser(UserData userToRemove)
        {
            foreach(var user in Users)
            {
                if (user.Equals(userToRemove))
                {
                    Users.Remove(user);
                    return;
                }
            }
        }
        public void AddUser(UserData userToAdd)
        {
            Users.Add(userToAdd);
        }
        public void RemoveManager(UserData managerToRemove)
        {
            GroupManagers.Remove(managerToRemove);
            foreach (var manager in GroupManagers)
            {
                if (manager.Equals(managerToRemove))
                {
                    GroupManagers.Remove(manager);
                    return;
                }
            }
        }
        public void AddManager(UserData managerToAdd)
        {
            GroupManagers.Add(managerToAdd);
        }
        public bool ContainsUser(UserData user)
        {
            foreach(var userInGroup in Users)
            {
                if (userInGroup.Equals(user))
                    return true;

            }
            return false;
        }

        public bool Equals(IMemberData member)
        {
            return Name == member.Name && UID == member.UID;
        }
    }
}
