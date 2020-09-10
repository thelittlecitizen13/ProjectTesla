using System;
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
        private ISerializer _binarySerializer;
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
            _binarySerializer = new BinarySerializer();
            _membersDB = new Members();
            _contactsDB = new Contacts();
            _AdminData = new UserData("Admin");
            ServerDTO = new ServerData(_localAddress, _server, _binarySerializer, _membersDB, _contactsDB, _AdminData);
            _messageSender = new MessageSender(_messageReceiver, ServerDTO);
            _messageReceiver = new MessageReceiver(_messageSender, ServerDTO);
            
    }
        
        private bool registerClient(TcpClient client)
        {
            NetworkStream nwStream = client.GetStream();
            TextMessage dataReceived = (TextMessage)_binarySerializer.Deserialize(nwStream);
            User newUser = new User((UserData)dataReceived.Source, client);
            string clientName = dataReceived.Source.Name;
            if (_membersDB.AddUser(newUser) && _contactsDB.AddUser((UserData)newUser.Data))
            {
                connectionEstablishedPrint(client, clientName);
                welcomeNewUser(newUser);
                return true;
            }
            else
            {
                TextMessage nameTakenMessage = new TextMessage($"{clientName} name is already taken", _AdminData, _AdminData);
                _binarySerializer.Serialize(nwStream, nameTakenMessage);
                return false;
            }
        }
        private void welcomeNewUser(User newUser)
        {
            TextMessage welcomeMessage = new TextMessage($"Welcome, {newUser.Name}", _AdminData, (UserData)newUser.Data);
            _binarySerializer.Serialize(newUser.nwStream, welcomeMessage);
            ContactsMessage newContactsDBMessage = new ContactsMessage(_contactsDB, _AdminData, _AdminData);
            _messageSender.SendToAllClients(newContactsDBMessage);
            TextMessage userJoinedChatMessage = new TextMessage($"{newUser.Name} joined the chat!", _AdminData, _AdminData);
            _messageSender.SendToAllClients(userJoinedChatMessage);
        }
        public void Run()
        {

            _server.Start();
            _contactsDB.AddUser(_AdminData);
            Console.WriteLine($"Listening at {_server.LocalEndpoint}. Waiting for connections.");
            try
            {
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
}
