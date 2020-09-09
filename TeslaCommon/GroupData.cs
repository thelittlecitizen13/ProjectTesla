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
        public void RemoveUser(UserData userToRemove)
        {
            Users.Remove(userToRemove);
        }
        public void AddUser(UserData userToAdd)
        {
            Users.Add(userToAdd);
        }
        public void RemoveManager(UserData managerToRemove)
        {
            GroupManagers.Remove(managerToRemove);
        }
        public void AddManager(UserData managerToAdd)
        {
            GroupManagers.Add(managerToAdd);
        }

    }
}
