﻿using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using TeslaCommon;

namespace TeslaServer
{
    public class MessageSender
    {
        private ServerData _serverDTO;
        public MessageSender(MessageReceiver messageReceiver, ServerData serverData)
        {
            _serverDTO = serverData;
        }
        public void SendToAllClients(object obj)
        {
            foreach (var user in _serverDTO.MembersDB.TeslaUsers.Values)
            {
                SendMessageToUser(user.nwStream, obj);
            }
        }
        public void SendNotAuthorizedMessage(UserData destination)
        {
            SendCustomMessage(destination, "You are not authorized to do this action!");
        }
        public void DeliverMessageToDestination(IMessage message)
        {
            // ToDo: Refactor - do logics in Members class

            if (message.GetType() == typeof(GroupMessage))
            {
                SendGroupMessage(message);
            }
            else
            //if (message.GetType() == typeof(TextMessage))
            {
                SendTextMessage(message);
            }

        }
        public void SendMessageToUser(NetworkStream nwStream, object obj)
        {
            Console.WriteLine($"Sending a message with type of {obj.GetType()}");
            _serverDTO.Serializer.Serialize(nwStream, obj);

        }
        public void SendTextMessage(IMessage message)
        {
            string destinationUID = message.Destination.UID;
            User destination = _serverDTO.MembersDB.GetUser(destinationUID);
            if (destination != null)
            {
                SendMessageToUser(destination.nwStream, message);
                return;
            }
        }
        public void SendGroupMessage(IMessage message)
        {
            if (message.Destination.Name == "Everyone")
            {
                SendToAllClients(message);
                return;
            }
            string destinationUID = message.Destination.UID;
            Group destination = _serverDTO.MembersDB.GetGroup(destinationUID);
            if (destination != null)
            {
                foreach (var userData in destination.GroupUsers)
                {
                    try
                    {
                        User userInGroup = _serverDTO.MembersDB.GetUser(userData.UID);
                        SendMessageToUser(userInGroup.nwStream, message);
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                return;
            }

        }
        public void UpdateUsersAboutGroupChange(GroupUpdateMessage message)
        {
            // updates group members about the change
            List<UserData> groupUsers = message.GroupChanged.Users;
            foreach (var groupMember in groupUsers)
            {
                User user = _serverDTO.MembersDB.GetUser(groupMember.UID);
                Console.WriteLine($"user {groupMember.Name} is in the group");
                if (user != null)
                {
                    Console.WriteLine($"user {user.Name} updated about group change");
                    _serverDTO.Serializer.Serialize(user.nwStream, message);
                }
            }
        }
        public void SendCustomMessage(IMemberData destinationUser, string msg)
        {
            User userInGroup = _serverDTO.MembersDB.GetUser(destinationUser.UID);
            TextMessage privateMessage = new TextMessage(msg, _serverDTO.AdminData, (UserData)destinationUser);
            SendMessageToUser(userInGroup.nwStream, privateMessage);
        }
    }
}
