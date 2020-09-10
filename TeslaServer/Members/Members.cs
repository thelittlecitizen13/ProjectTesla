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
                User deletedMember = null;
                TeslaUsers.Remove(teslaUserUID, out deletedMember);
                RemoveUserFromGroups(deletedMember);
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
                    User deletedMember = null;
                    TeslaUsers.Remove(clt.Key, out deletedMember);
                    RemoveUserFromGroups(deletedMember);
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
        public void RemoveUserFromGroups(User userToRemove)
        {
            if (userToRemove == null)
                return;
            foreach (var group in TeslaGroups.Values)
            {
                RemoveUserFromGroup((UserData)userToRemove.Data, group);
                RemoveManagerFromGroup((UserData)userToRemove.Data, group);

            }
        }
        public void RemoveUserFromGroup(UserData userToRemove, Group group)
        {
            bool isGroupContainsTheUser = false;
            int userIndexInGroup = 0;
            foreach (var userInGroup in group.GroupUsers)
            {

                if (userInGroup.UID == userToRemove.UID)
                {
                    isGroupContainsTheUser = true;
                    userIndexInGroup = group.GroupUsers.IndexOf(userInGroup);
                }
            }
            if (isGroupContainsTheUser)
            {
                group.GroupUsers.RemoveAt(userIndexInGroup);
            }
        }
        public void RemoveManagerFromGroup(UserData adminToRemove, Group group)
        {
            try
            {
                bool isGroupContainsTheUser = false;
                int adminIndexInGroup = 0;
                foreach (var adminInGroup in group.GroupManagers)
                {

                    if (adminInGroup.UID == adminToRemove.UID)
                    {
                        isGroupContainsTheUser = true;
                        adminIndexInGroup = group.GroupUsers.IndexOf(adminInGroup);
                    }
                }
                if (isGroupContainsTheUser)
                {
                    group.GroupUsers.RemoveAt(adminIndexInGroup);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }


    }
}
