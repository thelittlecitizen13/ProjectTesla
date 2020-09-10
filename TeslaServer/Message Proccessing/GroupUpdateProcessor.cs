using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TeslaCommon;

namespace TeslaServer
{
    public class GroupUpdateProcessor
    {
        private ServerData _serverDTO;
        private MessageSender _messageSender;
        public GroupUpdateProcessor(MessageSender messageSender ,ServerData serverData)
        {
            _serverDTO = serverData;
            _messageSender = messageSender;
        }
        public void ProcessGroupUpdateMessage(GroupUpdateMessage message)
        {

            switch (message.typeOfChange)
            {
                case ChangeType.Create:
                    createNewGroup(message);
                    break;
                case ChangeType.Update:
                    updateGroup(message);
                    break;
                case
                ChangeType.Leave:
                    leaveGroup(message);
                    break;
                case ChangeType.Delete:
                    removeGroup(message);
                    break;
                default:
                    _messageSender.SendCustomMessage(message.Source, "Bad request");
                    break;
            }
        }

        private void createNewGroup(GroupUpdateMessage message)
        {
            GroupData changedGroup = message.GroupChanged;
            _serverDTO.ContactsDB.AddGroup(changedGroup);
            Group newGroup = new Group(changedGroup);
            _serverDTO.MembersDB.AddGroup(newGroup);
            _messageSender.SendCustomMessage((UserData)message.Source, "Group created successfully.");
            _messageSender.UpdateUsersAboutGroupChange(message);
        }
        private void leaveGroup(GroupUpdateMessage message)
        {
            GroupData changedGroup = message.GroupChanged;
            removeUserFromGroup((UserData)message.Source, changedGroup);
            message.typeOfChange = ChangeType.Update;
            _serverDTO.MembersDB.UpdateGroup(changedGroup);
            _messageSender.SendCustomMessage((UserData)message.Source, "Left group successfully.");
            NotifyUsersAboutGroupLeave(changedGroup, (UserData)message.Source);
            _messageSender.UpdateUsersAboutGroupChange(message);
        }
        private void updateGroup(GroupUpdateMessage message)
        {
            GroupData changedGroup = message.GroupChanged;
            GroupData originalGroup = (GroupData)(_serverDTO.ContactsDB.GetContactByName(changedGroup.Name));
            if (_serverDTO.ContactsDB.TryUpdateGroup(changedGroup, (UserData)message.Source))
            {
                _serverDTO.MembersDB.UpdateGroup(changedGroup);
                _messageSender.SendCustomMessage((UserData)message.Source, "Group updated successfully.");
                
                _messageSender.UpdateUsersAboutGroupChange(message);
                notifyUsersAboutGroupKick(changedGroup, originalGroup);
                return;
            }
            _messageSender.SendNotAuthorizedMessage((UserData)message.Source);
        }
        public void NotifyUsersAboutGroupLeave(GroupData changedGroup, UserData userLeft)
        {
            GroupMessage userLeftGroupMessage = new GroupMessage($"{userLeft.Name} left the group", _serverDTO.AdminData, changedGroup, changedGroup);
            _messageSender.SendGroupMessage(userLeftGroupMessage);
        }
        private void notifyUsersAboutGroupKick(GroupData changedGroup, GroupData originalGroup)
        {
            List<UserData> removedUsers = originalGroup.Users.Where(user => !changedGroup.ContainsUser(user)).ToList();
            if (removedUsers != null)
            {
                GroupData removedUserDatasGroup = new GroupData(originalGroup.Name, _serverDTO.AdminData);
                removedUserDatasGroup.RemoveUser(_serverDTO.AdminData);
                GroupUpdateMessage deteleGroup = new GroupUpdateMessage(removedUserDatasGroup, ChangeType.Delete,
                    _serverDTO.AdminData, _serverDTO.AdminData); // Send a delete GroupUpdateMessage to removed users

                foreach (var removedUser in removedUsers)
                {
                    removeUserFromGroup(removedUser, originalGroup);
                    User user = _serverDTO.MembersDB.GetUser(removedUser.UID);
                    TextMessage userMessageOfRemoval = new TextMessage($"You kicked out of {changedGroup.Name} group!", 
                        _serverDTO.AdminData, (UserData)user.Data);
                    _messageSender.SendMessageToUser(user.nwStream, deteleGroup);
                    _messageSender.SendMessageToUser(user.nwStream, userMessageOfRemoval);
                    GroupMessage groupMessageOfRemoval = new GroupMessage($"{user.Name} kicked out of the group", _serverDTO.AdminData,
                        removedUserDatasGroup, removedUserDatasGroup);
                }
            }
        }
        private void removeUserFromGroup(UserData removedUser, GroupData group)
        {
            _serverDTO.ContactsDB.RemoveUserFromGroup(group, removedUser);
            Group groupUserLeft = _serverDTO.MembersDB.GetGroup(group.UID);
            _serverDTO.MembersDB.RemoveUserFromGroup(removedUser, groupUserLeft);
        }
        private void removeGroup(GroupUpdateMessage message)
        {

            GroupData changedGroup = message.GroupChanged;

            if (_serverDTO.ContactsDB.TryRemoveGroup(changedGroup, (UserData)message.Source))
            {

                _messageSender.UpdateUsersAboutGroupChange(message);
                _serverDTO.MembersDB.RemoveGroup(changedGroup.Name);
                _messageSender.SendCustomMessage((UserData)message.Source, "Group deleted successfully.");
                return;
            }
            _messageSender.SendNotAuthorizedMessage((UserData)message.Source);
        }
    }
}
