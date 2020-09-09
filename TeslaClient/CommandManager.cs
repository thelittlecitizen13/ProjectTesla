using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
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
            args = args.Select(str => str.ToLower()).ToArray();
            switch (args[0])
            {
                case "/group":
                    return generateGroupMessage(args);
                default:
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
                default:
                    return null;
            }
                    
        }
        private IMessage createNewGroupCommand(string groupName)
        {
            GroupData newGroup = new GroupData(groupName, _teslaClient.ClientData);
            return new GroupUpdateMessage(newGroup, ChangeType.Created, _teslaClient.ClientData, new UserData("Server"));
        }
        private IMessage removeGroupCommand(string groupName)
        {
            GroupData groupToRemove = (GroupData)_teslaClient.ContactsMan.GetContactByName(groupName);
            return new GroupUpdateMessage(groupToRemove, ChangeType.Deleted, _teslaClient.ClientData, new UserData("Server"));
        }
        private IMessage updateGroupCommand(string[] args)
        {
            // command : /group update groupname -updatetype users list (comma seperated)
            int indexOfAction = Array.IndexOf(args, "update");
            if (indexOfAction == -1)
                return null;
            GroupData groupData = (GroupData)_teslaClient.ContactsMan.GetContactByName(args[indexOfAction + 1]); // group name
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
        private IMessage addUsersToGroupCommand(GroupData groupData, List<UserData> users)
        {
            foreach (var user in users)
            {
                groupData.AddUser(user);
            }
            return new GroupUpdateMessage(groupData, ChangeType.Updated, _teslaClient.ClientData, new UserData("Server"));
            
        }
        private IMessage removeUsersFromGroupCommand(GroupData groupData, List<UserData> users)
        {
            foreach (var user in users)
            {
                groupData.RemoveUser(user);
            }
            return new GroupUpdateMessage(groupData, ChangeType.Updated, _teslaClient.ClientData, new UserData("Server"));

        }
        private IMessage addManagersToGroupCommand(GroupData groupData, List<UserData> users)
        {
            foreach (var user in users)
            {
                groupData.AddManager(user);
            }
            return new GroupUpdateMessage(groupData, ChangeType.Updated, _teslaClient.ClientData, new UserData("Server"));

        }
        private IMessage removeManagersFromGroupCommand(GroupData groupData, List<UserData> users)
        {
            foreach (var user in users)
            {
                groupData.RemoveManager(user);
            }
            return new GroupUpdateMessage(groupData, ChangeType.Updated, _teslaClient.ClientData, new UserData("Server"));

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
            sb.AppendLine("Note - you have to be the manager of the group to take those actions!");
            return sb.ToString();
        }
    }
}
