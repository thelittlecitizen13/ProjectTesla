using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using TeslaCommon;
using System.Linq;

namespace TeslaServer
{
    public class TeslaServer
    {
        private IPAddress _localAddress;
        private TcpListener _server;
        private IFormatter _binaryFormatter;
        private Members _membersDB;
        private Contacts _contactsDB;
        private UserData _AdminData;
        private MessageReceiver _messageReceiver;
        private MessageSender _messageSender;
        public ServerData ServerDTO;
        

        public TeslaServer(int port)
        {
            _localAddress = IPAddress.Parse("127.0.0.1");
            _server = new TcpListener(_localAddress, port);
            _binaryFormatter = new BinaryFormatter();
            _membersDB = new Members();
            _contactsDB = new Contacts();
            _AdminData = new UserData("Admin");
            ServerDTO = new ServerData(_localAddress, _server, _binaryFormatter, _membersDB, _contactsDB, _AdminData);
            _messageSender = new MessageSender(_messageReceiver, ServerDTO);
            _messageReceiver = new MessageReceiver(_messageSender, ServerDTO);
            
    }
        
        private bool registerClient(TcpClient client)
        {
            NetworkStream nwStream = client.GetStream();
            TextMessage dataReceived = (TextMessage)_binaryFormatter.Deserialize(nwStream);
            User newUser = new User((UserData)dataReceived.Source, client);
            string clientName = dataReceived.Source.Name;
            if (_membersDB.AddUser(newUser) && _contactsDB.AddUser((UserData)newUser.Data))
            {
                connectionEstablishedPrint(client, clientName);
                TextMessage welcomeMessage = new TextMessage($"Welcome, {clientName}", _AdminData, (UserData)newUser.Data);
                _binaryFormatter.Serialize(nwStream, welcomeMessage);
                ContactsMessage newContactsDBMessage = new ContactsMessage(_contactsDB, _AdminData, _AdminData);
                //deliverMessageToDestination(newContactsDBMessage);
                _messageSender.SendToAllClients(newContactsDBMessage);
                _messageSender.SendToAllClients(new TextMessage($"{clientName} joined the chat!", _AdminData, _AdminData));
                
                return true;
            }
            else
            {
                TextMessage nameTakenMessage = new TextMessage($"{clientName} name is already taken", _AdminData, _AdminData);
                _binaryFormatter.Serialize(nwStream, nameTakenMessage);
                return false;
            }
        }
        public void Run()
        {

            _server.Start();
            _contactsDB.AddUser(_AdminData);
            Console.WriteLine($"Listening at {_server.LocalEndpoint}. Waiting for connections.");
            try
            {
                // ToDo: Figure a way to accept client connections async at the best way.
                while (true)
                {
                    //---incoming client connected---
                    TcpClient client = _server.AcceptTcpClient();
                    object obj = new object();
                    ThreadPool.QueueUserWorkItem(obj =>
                    {
                        if (registerClient(client))
                        {
                            _messageReceiver.ReceiveMessage(client);
                        }
                        else
                        {
                            client.Close();
                        }
                    }, null);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                Console.WriteLine("Terminating...");
                _server.Stop();
            }
        }      
        private void connectionEstablishedPrint(TcpClient client, string Name)
        {
            Console.WriteLine($"{Name} is connected. Remote connection: {0}:{1} ",
                        ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString(),
                        ((IPEndPoint)client.Client.RemoteEndPoint).Port.ToString());
        }
    }
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
                Console.WriteLine($"message sent to {message.Source.Name}"); //debug
            }
            else
            //if (message.GetType() == typeof(TextMessage))
            {
                SendTextMessage(message);
                Console.WriteLine($"message sent to {message.Source.Name}"); //debug
            }

        }
        public void SendMessageToUser(NetworkStream nwStream, object obj)
        {
            Console.WriteLine($"Sending a message with type of {obj.GetType()}");
            _serverDTO.BinarySerializer = new BinaryFormatter();
            _serverDTO.BinarySerializer.Serialize(nwStream, obj);

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

            Console.WriteLine("No such user"); //Debugging
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
                //Group destinationGroup = (Group)destination;
                foreach (var userData in destination.GroupUsers)
                {
                    User userInGroup = _serverDTO.MembersDB.GetUser(userData.UID);
                    SendMessageToUser(userInGroup.nwStream, message);
                }
                return;
            }
            Console.WriteLine("No such Group"); //Debugging

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
                    _serverDTO.BinarySerializer = new BinaryFormatter();
                    _serverDTO.BinarySerializer.Serialize(user.nwStream, message);
                }
            }
        }
        public void SendCustomMessage(IMemberData destinationUser, string msg)
        {
            User userInGroup = _serverDTO.MembersDB.GetUser(destinationUser.UID);
            TextMessage badRequestMessage = new TextMessage(msg, _serverDTO.AdminData, (UserData)destinationUser);
            SendMessageToUser(userInGroup.nwStream, badRequestMessage);
        }
    }
}
