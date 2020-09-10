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
        private MessageReceiver _messageReceiver;
        private MessageSender _messageSender;
        private TcpListener _server;
        public ServerData ServerDTO;


        public TeslaServer(int port)
        {
            _localAddress = IPAddress.Parse("127.0.0.1");
            _server = new TcpListener(_localAddress, port);
            createServerData();
            _messageSender = new MessageSender(_messageReceiver, ServerDTO);
            _messageReceiver = new MessageReceiver(_messageSender, ServerDTO);

        }
        private void createServerData()
        {
            
            Members membersDB = new Members();
            Contacts contactsDB = new Contacts();
            UserData AdminData = new UserData("Admin");
            ISerializer serializer = new BinarySerializer();
            ServerDTO = new ServerData(serializer, membersDB, contactsDB, AdminData);
        }
        
        private bool registerClient(TcpClient client)
        {
            NetworkStream nwStream = client.GetStream();
            TextMessage dataReceived = (TextMessage)(ServerDTO.Serializer.Deserialize(nwStream));
            User newUser = new User((UserData)dataReceived.Source, client);
            string clientName = dataReceived.Source.Name;
            if (ServerDTO.MembersDB.AddUser(newUser) && ServerDTO.ContactsDB.AddUser((UserData)newUser.Data))
            {
                connectionEstablishedPrint(client, clientName);
                welcomeNewUser(newUser);
                return true;
            }
            else
            {
                TextMessage nameTakenMessage = new TextMessage($"{clientName} name is already taken", ServerDTO.AdminData, ServerDTO.AdminData);
                ServerDTO.Serializer.Serialize(nwStream, nameTakenMessage);
                return false;
            }
        }
        private void welcomeNewUser(User newUser)
        {
            TextMessage welcomeMessage = new TextMessage($"Welcome, {newUser.Name}", ServerDTO.AdminData, (UserData)newUser.Data);
            ServerDTO.Serializer.Serialize(newUser.nwStream, welcomeMessage);
            ContactsMessage newContactsDBMessage = new ContactsMessage(ServerDTO.ContactsDB, ServerDTO.AdminData, ServerDTO.AdminData);
            _messageSender.SendToAllClients(newContactsDBMessage);
            TextMessage userJoinedChatMessage = new TextMessage($"{newUser.Name} joined the chat!", ServerDTO.AdminData, ServerDTO.AdminData);
            _messageSender.SendToAllClients(userJoinedChatMessage);
        }
        public void Run()
        {

            _server.Start();
            ServerDTO.ContactsDB.AddUser(ServerDTO.AdminData);
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
