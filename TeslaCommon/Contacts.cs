using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace TeslaCommon
{
    [Serializable]
    public class Contacts
    {
        public Dictionary<string, UserData> UsersList { get; set; }
        public Dictionary<string, GroupData> GroupsList { get; set; }
        public Contacts()
        {
            UsersList = new Dictionary<string, UserData>();
            GroupsList = new Dictionary<string, GroupData>();
            GroupsList.TryAdd("Everyone", new GroupData("Everyone", new UserData("Admin")));
        }
        public bool AddUser(UserData memberData)
        {
            return UsersList.TryAdd(memberData.Name, memberData);
        }
        public void RemoveUser(UserData memberData)
        {
            if (UsersList.ContainsKey(memberData.Name))
            {
                UserData removedMemberData;
                UsersList.Remove(memberData.Name, out removedMemberData);
            }
        }
        public bool AddGroup(GroupData memberData)
        {
            return GroupsList.TryAdd(memberData.Name, memberData);
        }
        public void RemoveGroup(GroupData memberData)
        {
            if (GroupsList.ContainsKey(memberData.Name))
            {
                GroupData removedMemberData;
                GroupsList.Remove(memberData.Name, out removedMemberData);
            }
        }
    }
}
