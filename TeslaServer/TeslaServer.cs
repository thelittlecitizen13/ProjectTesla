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
                ContactsMessage newContactsDBMessage = new ContactsMessage(_contactsDB, new UserData("Server"), _contactsDB.UsersList["Everyone"]);
                deliverMessageToDestination(newContactsDBMessage);
                SendToAllClients(new TextMessage($"{clientName} joined the chat!", new UserData("Server"), new UserData("all")));
                
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
            if (message.GetType() != typeof(CommandMessage))
                deliverMessageToDestination(message);
            // ToDo: Handle command messages
        }
        private void deliverMessageToDestination(IMessage message)
        {
            // ToDo: Refactor - do logics in Members class
            if (message.Destination.Name == "Everyone")
            {
                SendToAllClients(message);
                return;
            }
            string destinationUID = message.Destination.UID;
            IMember destination;
            destination = _membersDB.GetUser(destinationUID);
            if (destination != null)
            {
                User destinationUser = (User)destination;
                sendMessageToUser(destinationUser.nwStream, message);
                return;
            }
            destination = _membersDB.GetGroup(destinationUID);
            if (destination != null)
            {
                Group destinationGroup = (Group)destination;
                foreach (var userData in destinationGroup.GroupUsers)
                {
                    User userInGroup = _membersDB.GetUser(userData.UID);
                    sendMessageToUser(userInGroup.nwStream, message);
                }
                return;
            }
            Console.WriteLine("No such member"); //Debugging
        }
        private void sendMessageToUser(NetworkStream nwStream, object obj)
        {
            Console.WriteLine($"Sending a message with type of {obj.GetType()}");
            _binaryFormatter = new BinaryFormatter();
            _binaryFormatter.Serialize(nwStream, obj);

        }


    }
}
