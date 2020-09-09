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

namespace TeslaServer
{
    public class TeslaServer
    {
        private IPAddress _localAddress;
        private TcpListener _server;
        private IFormatter _binaryFormatter;
        private Members _membersDB;
        private Contacts _contactsDB;

        public TeslaServer(int port)
        {
            _localAddress = IPAddress.Parse("127.0.0.1");
            _server = new TcpListener(_localAddress, port);
            _binaryFormatter = new BinaryFormatter();
            _membersDB = new Members();
            _contactsDB = new Contacts();
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
                TextMessage welcomeMessage = new TextMessage($"Welcome, {clientName}", new UserData("Server"), new UserData("all"));
                _binaryFormatter.Serialize(nwStream, welcomeMessage);
                ContactsMessage newContactsDBMessage = new ContactsMessage(_contactsDB, new UserData("Server"), new UserData("all")); // refactor!!
                //deliverMessageToDestination(newContactsDBMessage);
                SendToAllClients(newContactsDBMessage);
                SendToAllClients(new TextMessage($"{clientName} joined the chat!", new UserData("Server"), new UserData("all"))); //refactor!!
                
                return true;
            }
            else
            {
                TextMessage nameTakenMessage = new TextMessage($"{clientName} name is already taken", new UserData("all"), new UserData("all"));
                _binaryFormatter.Serialize(nwStream, nameTakenMessage);
                return false;
            }
        }
        public void Run()
        {

            _server.Start();
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
                            receiveMessage(client);
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


        private void removeUserFromMembersDB(TcpClient client)
        {
            User removedUser = _membersDB.RemoveUser(client);
            _contactsDB.RemoveUser((UserData)removedUser.Data);
            if (removedUser != null)
            {
                SendToAllClients(new TextMessage($"{removedUser.Name} has left the chat!", new UserData("Server"), new UserData("all")));
                SendToAllClients(new ContactsMessage(_contactsDB, new UserData("Server"), new UserData("all"))); // ToDo: move to a function with indicative name
            }
        }
        private void connectionEstablishedPrint(TcpClient client, string Name)
        {
            Console.WriteLine($"{Name} is connected. Remote connection: {0}:{1} ",
                        ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString(),
                        ((IPEndPoint)client.Client.RemoteEndPoint).Port.ToString());
        }
        
        private void SendToAllClients(object obj)
        {
            foreach (var user in _membersDB.TeslaUsers.Values)
            {
                sendMessageToUser(user.nwStream, obj);
            }
        }
        
        private void receiveMessage(TcpClient client) 
        {
            //---get the incoming data through a network stream---
            NetworkStream nwStream = client.GetStream();
            do
            {
                try
                {
                    var dataReceived = _binaryFormatter.Deserialize(nwStream);
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
                // ToDo: Handle command messages
                return;
            }
            deliverMessageToDestination(message);


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
                case ChangeType.Leave:
                    leaveGroup(message);
                    break;
                case ChangeType.Delete:
                    removeGroup(message);
                    break;
                default:
                    sendCustomMessage(message.Source, "Bad request");
                    break;
            }
            // ToDo: update groups
        }
        private void updateUsersAboutGroupChange(GroupUpdateMessage message)
        {
            // updates group members about the change
            List<UserData> groupUsers = message.GroupChanged.Users;
            foreach (var groupMember in groupUsers)
            {
                User user = _membersDB.GetUser(groupMember.UID);
                Console.WriteLine($"user {groupMember.Name} is in the group");
                if (user != null)
                {
                    Console.WriteLine($"user {user.Name} updated about group change");
                    _binaryFormatter = new BinaryFormatter();
                    _binaryFormatter.Serialize(user.nwStream, message);
                }
            }
        }
        private void sendCustomMessage(IMemberData destinationUser, string msg)
        {
            User userInGroup = _membersDB.GetUser(destinationUser.UID);
            TextMessage badRequestMessage = new TextMessage(msg, new UserData("Server"), (UserData)destinationUser);
            sendMessageToUser(userInGroup.nwStream, badRequestMessage);
        }
        private void createNewGroup(GroupUpdateMessage message)
        {
            GroupData changedGroup = message.GroupChanged;
            _contactsDB.AddGroup(changedGroup);
            sendCustomMessage((UserData)message.Source, "Group created successfully.");
            updateUsersAboutGroupChange(message);
        }
        private void leaveGroup(GroupUpdateMessage message)
        {
            GroupData changedGroup = message.GroupChanged;
            _contactsDB.RemoveUserFromGroup(changedGroup, (UserData)message.Source);
            sendCustomMessage((UserData)message.Source, "Left group successfully.");
            message.typeOfChange = ChangeType.Update;
            updateUsersAboutGroupChange(message);
        }
        private void updateGroup(GroupUpdateMessage message)
        {
            GroupData changedGroup = message.GroupChanged;
            if(_contactsDB.TryUpdateGroup(changedGroup, (UserData)message.Source))
            {
                sendCustomMessage((UserData)message.Source, "Group updated successfully.");
                updateUsersAboutGroupChange(message);
                return;
            }
            sendNotAuthorizedMessage((UserData)message.Source);
        }
        private void removeGroup(GroupUpdateMessage message)
        {
            
            GroupData changedGroup = message.GroupChanged;
            
            if (_contactsDB.TryRemoveGroup(changedGroup, (UserData)message.Source))
            {
                updateUsersAboutGroupChange(message);
                sendCustomMessage((UserData)message.Source, "Group deleted successfully.");
                return;
            }
            sendNotAuthorizedMessage((UserData)message.Source);
        }
        private void sendNotAuthorizedMessage(UserData destination)
        {
            sendCustomMessage(destination, "You are not authorized to do this action!");
        }
        private void deliverMessageToDestination(IMessage message)
        {
            // ToDo: Refactor - do logics in Members class

            if (message.GetType() == typeof(GroupMessage))
            {
                sendGroupMessage(message);
                Console.WriteLine($"message sent to {message.Source.Name}"); //debug
            }
            else
            //if (message.GetType() == typeof(TextMessage))
            {
                sendTextMessage(message);
                Console.WriteLine($"message sent to {message.Source.Name}"); //debug
            }

        }
        private void sendMessageToUser(NetworkStream nwStream, object obj)
        {
            Console.WriteLine($"Sending a message with type of {obj.GetType()}");
            _binaryFormatter = new BinaryFormatter();
            _binaryFormatter.Serialize(nwStream, obj);

        }
        private void sendTextMessage(IMessage message)
        {
            string destinationUID = message.Destination.UID;
            User destination = _membersDB.GetUser(destinationUID);
            if (destination != null)
            {
                sendMessageToUser(destination.nwStream, message);
                return;
            }

            Console.WriteLine("No such user"); //Debugging
        }
        private void sendGroupMessage(IMessage message)
        {
            if (message.Destination.Name == "Everyone")
            {
                SendToAllClients(message);
                return;
            }
            string destinationUID = message.Destination.UID;
            Group destination = _membersDB.GetGroup(destinationUID);
            if (destination != null)
            {
                //Group destinationGroup = (Group)destination;
                foreach (var userData in destination.GroupUsers)
                {
                    User userInGroup = _membersDB.GetUser(userData.UID);
                    sendMessageToUser(userInGroup.nwStream, message);
                }
                return;
            }
            Console.WriteLine("No such Group"); //Debugging

        }


    }
}
