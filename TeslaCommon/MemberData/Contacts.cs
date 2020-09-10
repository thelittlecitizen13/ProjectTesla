using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

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
            try
            {
                UserData removedMemberData = null;
                if (UsersList.ContainsKey(memberData.Name))
                {
                    UsersList.Remove(memberData.Name, out removedMemberData);
                }
                if (removedMemberData != null)
                {
                    List<GroupData> userGroupMembership = GetUserGroupMembership(memberData);
                    foreach (var group in userGroupMembership)
                    {
                        RemoveUserFromGroup(group, removedMemberData);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message) ;
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
        public bool TryUpdateGroup(GroupData groupData, UserData commiter)
        {
            if (!checkUserPermissionsOnGroup(commiter, groupData))
            {
                return false;
            }
            try
            {
                GroupsList[groupData.Name] = groupData;
                return true;
            }
            catch
            {
                Console.WriteLine("Group update failed");
                return false;
            }
        }
        public bool TryRemoveGroup(GroupData groupData, UserData commiter)
        {
            
            if (!checkUserPermissionsOnGroup(commiter, groupData))
            {
                return false;
            }
            try
            {
                RemoveGroup(groupData);
                return true;
            }
            catch
            {
                Console.WriteLine("Group update failed");
                return false;
            }
        }
        private bool checkUserPermissionsOnGroup(UserData user, GroupData group)
        {
            GroupData originalGroup = (GroupData)GetContactByName(group.Name);
            foreach (var manager in originalGroup.GroupManagers)
            {
                if (manager.Equals(user))
                    return true;
            }
            return false;
        }
        public IMemberData GetContactByName(string name)
        {
            if (UsersList.ContainsKey(name))
                return UsersList[name];
            if (GroupsList.ContainsKey(name))
                return GroupsList[name];
            return null;
        }
        public void RemoveUserFromGroup(GroupData groupData, UserData commiter)
        {
            UserData userToRemove = null; 
            foreach (var user in groupData.Users)
            {
                if (commiter.Equals(user))
                {
                    userToRemove = user;
                }

            }
            if (userToRemove != null)
                groupData.Users.Remove(userToRemove);
            userToRemove = null;
            foreach (var manager in groupData.GroupManagers)
            {
                if (commiter.Equals(manager))
                {
                    userToRemove = manager;
                }
            }
            if (userToRemove != null)
                groupData.GroupManagers.Remove(userToRemove);
            GroupsList[groupData.Name] = groupData;

        }
        public List<GroupData> GetUserGroupMembership(UserData userData)
        {
            // REAL SHIT!! there was no time. REFACTOR!
            List<GroupData> groupMembership = new List<GroupData>();
            foreach (var groupData in GroupsList.Values)
            {
                foreach (var user in groupData.Users)
                {
                    if (user.Equals(userData))
                    {
                        groupMembership.Add(groupData);
                        break;
                    }
                }
            }
            foreach (var groupData in GroupsList.Values)
            {
                foreach (var manager in groupData.GroupManagers)
                {
                    if (manager.Equals(userData))
                    {
                        groupMembership.Add(groupData);
                    }
                }
            }
            List<GroupData> uniqueGroupsMembership = groupMembership.GroupBy(x => x.UID).Select(group => group.First()).ToList();
            return uniqueGroupsMembership;
        }
        
    }
}
