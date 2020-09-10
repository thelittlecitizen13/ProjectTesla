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
            // ToDo: update groups
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
            _serverDTO.ContactsDB.RemoveUserFromGroup(changedGroup, (UserData)message.Source);
            _messageSender.SendCustomMessage((UserData)message.Source, "Left group successfully.");
            message.typeOfChange = ChangeType.Update;
            _serverDTO.MembersDB.UpdateGroup(changedGroup);
            _messageSender.UpdateUsersAboutGroupChange(message);
        }
        private void updateGroup(GroupUpdateMessage message)
        {
            GroupData changedGroup = message.GroupChanged;
            GroupData oldGroup = (GroupData)(_serverDTO.ContactsDB.GetContactByName(changedGroup.Name));
            if (_serverDTO.ContactsDB.TryUpdateGroup(changedGroup, (UserData)message.Source))
            {
                _serverDTO.MembersDB.UpdateGroup(changedGroup);
                _messageSender.SendCustomMessage((UserData)message.Source, "Group updated successfully.");
                List<UserData> removedUsers = oldGroup.Users.Where(user => !changedGroup.ContainsUser(user)).ToList();
                _messageSender.UpdateUsersAboutGroupChange(message);
                if (removedUsers != null)
                {
                    GroupData removedUserDatasGroup = new GroupData(oldGroup.Name, _serverDTO.AdminData);
                    removedUserDatasGroup.RemoveUser(_serverDTO.AdminData);
                    GroupUpdateMessage messageOfRemoval = new GroupUpdateMessage(removedUserDatasGroup, ChangeType.Delete, _serverDTO.AdminData, _serverDTO.AdminData);
                    foreach (var removedUser in removedUsers)
                    {
                        User user = _serverDTO.MembersDB.GetUser(removedUser.UID);
                        _messageSender.SendMessageToUser(user.nwStream, messageOfRemoval);
                    }
                }
                return;
            }
            _messageSender.SendNotAuthorizedMessage((UserData)message.Source);
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
