using System;
using System.Collections.Generic;
using System.Net.Sockets;
using TeslaCommon;
using System.Linq;

namespace TeslaServer
{
    public class MessageReceiver
    {
        private MessageSender _messageSender;
        private ServerData _serverDTO;
        public MessageReceiver(MessageSender messageSender, ServerData serverData)
        {
            _messageSender = messageSender;
            _serverDTO = serverData;
        }
        public void ReceiveMessage(TcpClient client)
        {
            //---get the incoming data through a network stream---
            NetworkStream nwStream = client.GetStream();
            do
            {
                try
                {
                    var dataReceived = _serverDTO.BinarySerializer.Deserialize(nwStream);
                    processMessage((IMessage)dataReceived);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    break;
                }
            }
            while (true);
            // ToDo: find a way to change it - maybe to create searlization class
            //while (!dataReceived.ToLower().Contains("exit!"));
            removeUserFromMembersDB(client);
            client.Close();
        }
        private void processMessage(IMessage message)
        {
            if (message.GetType() == typeof(GroupUpdateMessage))
            {
                processGroupUpdateMessage((GroupUpdateMessage)message);
                return;
            }
            if (message.GetType() == typeof(CommandMessage))
            {
                // ToDo: Handle command messages when implemented
                return;
            }
            _messageSender.DeliverMessageToDestination(message);


        }
        private void processGroupUpdateMessage(GroupUpdateMessage message)
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
                //message.GroupChanged = oldGroup;
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
        private void removeUserFromMembersDB(TcpClient client)
        {
            User removedUser = _serverDTO.MembersDB.RemoveUser(client);
            _serverDTO.ContactsDB.RemoveUser((UserData)removedUser.Data);
            if (removedUser != null)
            {
                _messageSender.SendToAllClients(new TextMessage($"{removedUser.Name} has left the chat!", _serverDTO.AdminData, _serverDTO.AdminData));
                _messageSender.SendToAllClients(new ContactsMessage(_serverDTO.ContactsDB, _serverDTO.AdminData, _serverDTO.AdminData)); // ToDo: move to a function with indicative name
            }
        }
    }
}
