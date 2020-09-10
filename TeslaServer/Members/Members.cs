using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using TeslaCommon;

namespace TeslaServer
{
    [Serializable]    
    public class Members
    {
        public Dictionary<string, User> TeslaUsers { get; set; }
        public Dictionary<string, Group> TeslaGroups { get; set; }
        public Members()
        {
            TeslaUsers = new Dictionary<string, User>();
            TeslaGroups = new Dictionary<string, Group>();
        }
        public bool AddUser(User member)
        {
            return TeslaUsers.TryAdd(member.Data.UID, member);
        }
        public User RemoveUser(string memberName)
        {
            string teslaUserUID = TeslaUsers.Where(user => user.Value.Data.Name == memberName).FirstOrDefault().Key ?? "";
            if (!string.IsNullOrWhiteSpace(teslaUserUID))
            {
                User deletedMember;
                TeslaUsers.Remove(teslaUserUID, out deletedMember);
                return deletedMember;
            }
            return null;
        }
        public User RemoveUser(TcpClient client)
        {
            foreach (var clt in TeslaUsers)
            {
                if (clt.Value.client == client)
                {
                    User deletedMember;
                    TeslaUsers.Remove(clt.Key, out deletedMember);
                    return deletedMember;
                }
            }
            return null;
        }
        public bool AddGroup(Group group)
        {
            return TeslaGroups.TryAdd(group.Data.UID, group);
        }
        public Group RemoveGroup(string memberName)
        {
            string teslaUserUID = TeslaGroups.Where(user => user.Value.Data.Name == memberName).FirstOrDefault().Key ?? "";
            if (!string.IsNullOrWhiteSpace(teslaUserUID))
            {
                Group deletedMember;
                TeslaGroups.Remove(teslaUserUID, out deletedMember);
                return deletedMember;
            }
            return null;
        }
        public User GetUser(string userUID)
        {
            if (TeslaUsers.ContainsKey(userUID))
                return TeslaUsers[userUID];
            return null;
        }
        public Group GetGroup(string GroupUID)
        {
            if (TeslaGroups.ContainsKey(GroupUID))
                return TeslaGroups[GroupUID];
            return null;
        }
        public bool UpdateGroup(GroupData groupData)
        {
            try
            {
                TeslaGroups[groupData.UID] = new Group(groupData);
                return true;
            }
            catch
            {
                return false;
            }
            
        }


    }
}
