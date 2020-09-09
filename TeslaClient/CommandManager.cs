using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using TeslaCommon;

namespace TeslaClient
{
    public class CommandManager
    {
        private TeslaClient _teslaClient;
        public CommandManager(TeslaClient teslaClient)
        {
            _teslaClient = teslaClient;
        }
        //public bool IsCommandValid()
        //{

        //}
        public IMessage GenerateMessageFromCommand(string[] args)
        {
            try
            {
                args = args.Select(str => str.ToLower()).ToArray();
                switch (args[0])
                {
                    case "/group":
                        return generateGroupMessage(args);
                    default:
                        return null;
                }
            }
            catch
            {
                return null;
            }
        }
        private IMessage generateGroupMessage(string[] args)
        {
            switch (args[1].ToLower())
            { 
                case "create":
                    return createNewGroupCommand(args[2]); // args[2] = group name
                case "update":
                    return updateGroupCommand(args);
                case "remove":
                    return removeGroupCommand(args[2]);
                case "leave":
                    return leaveGroupCommand(args[2]);
                default:
                    return null;
            }
                    
        }
        private IMessage createNewGroupCommand(string groupName)
        {
            GroupData newGroup = new GroupData(groupName, _teslaClient.ClientData);
            if (newGroup != null)
                return new GroupUpdateMessage(newGroup, ChangeType.Create, _teslaClient.ClientData, new UserData("Server"));
            return null;
        }
        private IMessage removeGroupCommand(string groupName)
        {
            GroupData groupToRemove = (GroupData)_teslaClient.ContactsMan.GetContactByName(groupName);
            if (groupToRemove != null)
                return new GroupUpdateMessage(groupToRemove, ChangeType.Delete, _teslaClient.ClientData, new UserData("Server"));
            return null;
        }
        private IMessage leaveGroupCommand(string groupName)
        {
            GroupData groupToLeave = (GroupData)_teslaClient.ContactsMan.GetContactByName(groupName);
            if (groupToLeave != null)
            {
                groupToLeave.RemoveUser(_teslaClient.ClientData);
                groupToLeave = (GroupData)_teslaClient.ContactsMan.GetContactByName(groupName);
                _teslaClient.ContactsMan.ContactsDB.RemoveGroup(groupToLeave);
                
                Console.WriteLine("Leaving group..."); //debug
                return new GroupUpdateMessage(groupToLeave, ChangeType.Leave, _teslaClient.ClientData, new UserData("Server"));
            }
            return null;
        }
        private IMessage updateGroupCommand(string[] args)
        {
            // command : /group update groupname -updatetype users list (comma seperated)
            int indexOfAction = Array.IndexOf(args, "update");
            if (indexOfAction == -1)
                return null;
            GroupData groupData = (GroupData)_teslaClient.ContactsMan.GetContactByName(args[indexOfAction + 1]); // group name
            if (groupData != null)
            {
                int indexOfUpdateType = indexOfAction + 2;
                int indexOfUsers = indexOfAction + 3;
                List<UserData> usersListed = new List<UserData>();
                for (int i = indexOfUsers; i < args.Length; i++)
                {
                    UserData user = (UserData)_teslaClient.ContactsMan.GetContactByName(args[i]);
                    if (user != null)
                        usersListed.Add(user);
                }
                switch (args[indexOfUpdateType].ToLower())
                {
                    case "-addusers":
                        return addUsersToGroupCommand(groupData, usersListed);
                    case "-removeusers":
                        return removeUsersFromGroupCommand(groupData, usersListed);
                    case "-addmanagers":
                        return addManagersToGroupCommand(groupData, usersListed);
                    case "-removemanagers":
                        return removeManagersFromGroupCommand(groupData, usersListed);
                    default:
                        return null;
                }
            }
            return null;
        }
        private IMessage addUsersToGroupCommand(GroupData groupData, List<UserData> users)
        {
            GroupData groupToAddAUser = groupData;
            foreach (var user in users)
            {
                groupToAddAUser.AddUser(user);
            }
            return new GroupUpdateMessage(groupData, ChangeType.Update, _teslaClient.ClientData, new UserData("Server"));
            
        }
        private IMessage removeUsersFromGroupCommand(GroupData groupData, List<UserData> users)
        {
            foreach (var user in users)
            {
                groupData.RemoveUser(user);
            }
            return new GroupUpdateMessage(groupData, ChangeType.Update, _teslaClient.ClientData, new UserData("Server"));

        }
        private IMessage addManagersToGroupCommand(GroupData groupData, List<UserData> users)
        {
            foreach (var user in users)
            {
                groupData.AddManager(user);
            }
            return new GroupUpdateMessage(groupData, ChangeType.Update, _teslaClient.ClientData, new UserData("Server"));

        }
        private IMessage removeManagersFromGroupCommand(GroupData groupData, List<UserData> users)
        {
            foreach (var user in users)
            {
                groupData.RemoveManager(user);
            }
            return new GroupUpdateMessage(groupData, ChangeType.Update, _teslaClient.ClientData, new UserData("Server"));

        }
        public string GetCommandHelp()
        {
            return GetGroupCommandHelp();
        }
        public string GetGroupCommandHelp()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("Usage of /group command:");
            sb.AppendLine("Create a group:");
            sb.AppendLine("/group create [GroupName]");
            sb.AppendLine();
            sb.AppendLine("Update a group:");
            sb.AppendLine("/group update [GroupName] [-addusers | -removeusers | -addmanagers | -removemanagers] [users list (space seperated)]");
            sb.AppendLine();
            sb.AppendLine("Remove a group:");
            sb.AppendLine("/group remove [GroupName]");
            sb.AppendLine();
            sb.AppendLine("Leave a group:");
            sb.AppendLine("/group leave [GroupName]");
            sb.AppendLine();
            sb.AppendLine("Note - you have to be the manager of the group to take those actions!");
            return sb.ToString();
        }
    }
}
