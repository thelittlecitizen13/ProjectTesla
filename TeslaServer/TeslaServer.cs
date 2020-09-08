using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using TeslaCommon;

namespace TeslaServer
{
    public class TeslaServer
    {
        private IPAddress _localAddress;
        private TcpListener _server;
        private ConcurrentDictionary<string, TcpClient> _clientsList;
        private IFormatter _binaryFormatter;

        public TeslaServer(int port)
        {
            _localAddress = IPAddress.Parse("127.0.0.1");
            _server = new TcpListener(_localAddress, port);
            _clientsList = new ConcurrentDictionary<string, TcpClient>();
            _binaryFormatter = new BinaryFormatter();
        }
        private bool registerClient(TcpClient client)
        {
            NetworkStream nwStream = client.GetStream();
            byte[] buffer = new byte[client.ReceiveBufferSize];

            //---read incoming stream---
            int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);

            //---convert the data received into a string---
            string clientName = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            if (tryAddClientToList(clientName, client))
            {
                byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes($"Welcome, {clientName}");
                connectionEstablishedPrint(client, clientName);
                //---send the text---
                nwStream.Write(bytesToSend, 0, bytesToSend.Length);
                SendToAllClients($"{clientName} joined!");
                return true;
            }
            else
            {
                byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes($"{clientName} name is already taken");
                //---send the text---
                nwStream.Write(bytesToSend, 0, bytesToSend.Length);
                return false;
            }

        }
        private bool registerClientWithMessage(TcpClient client)
        {
            NetworkStream nwStream = client.GetStream();
            TextMessage dataReceived = (TextMessage)_binaryFormatter.Deserialize(nwStream);
            string clientName = dataReceived.Source.ClientName;
            if (tryAddClientToList(clientName, client))
            {
                connectionEstablishedPrint(client, clientName);
                TextMessage welcomeMessage = new TextMessage($"Welcome, {clientName}", new ClientData("all"), new ClientData("all"));
                _binaryFormatter.Serialize(nwStream, welcomeMessage);
                SendToAllClients(new TextMessage($"{clientName} joined the chat!", new ClientData("Server"), new ClientData("all")));
                return true;
            }
            else
            {
                TextMessage nameTakenMessage = new TextMessage($"{clientName} name is already taken", new ClientData("all"), new ClientData("all"));
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
                        if (registerClientWithMessage(client))
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
        private bool tryAddClientToList(string name, TcpClient client)
        {
            if (!_clientsList.ContainsKey(name))
            {
                _clientsList.TryAdd(name, client);
                return true;
            }
            return false;

        }
        private void removeClientFromList(TcpClient client)
        {

            string clientName = "";
            foreach (var clt in _clientsList)
            {
                if (clt.Value == client)
                {
                    clientName = clt.Key;
                    _clientsList.TryRemove(clientName, out client);
                    Console.WriteLine($"{clientName} left the chat!");
                    break;
                }
            }
            if (!string.IsNullOrWhiteSpace(clientName))
                SendToAllClients(new TextMessage($"{clientName} has left the chat!", new ClientData("Server"), new ClientData("all")));
        }
        private void connectionEstablishedPrint(TcpClient client, string Name)
        {
            Console.WriteLine($"{Name} is connected. Remote connection: {0}:{1} ",
                        ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString(),
                        ((IPEndPoint)client.Client.RemoteEndPoint).Port.ToString());
        }
        private void SendToAllClients(string msg)
        {
            byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(msg);
            foreach (var client_port in _clientsList)
            {
                NetworkStream nwStream = client_port.Value.GetStream();

                nwStream.Write(bytesToSend, 0, bytesToSend.Length);
            }
        }
        private void SendToAllClients(object obj)
        {
            foreach (var client_port in _clientsList)
            {
                NetworkStream nwStream = client_port.Value.GetStream();
                _binaryFormatter.Serialize(nwStream, obj);
                //nwStream.Write(bytesToSend, 0, bytesToSend.Length);
            }
        }
        private void receiveMessagesAsText(TcpClient client)
        {
            //---get the incoming data through a network stream---
            NetworkStream nwStream = client.GetStream();
            byte[] buffer = new byte[client.ReceiveBufferSize];

            string dataReceived;
            //ToDo: split text send and recieve to a different function, before changing to receiving objects
            do
            {
                //---read incoming stream---
                try
                {
                    int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);
                    //---convert the data received into a string---
                    dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    Console.WriteLine("Received : " + dataReceived);

                    //---write back the text to the client---
                    Console.WriteLine("Sending to all clients : " + dataReceived);
                    SendToAllClients(dataReceived);
                }
                catch
                {
                    break;
                }


            }
            while (!dataReceived.ToLower().Contains("exit!"));
            //ToDo: to send & recieve repeatedly, should find a way to loop the send & receive 
            //      and take the client.close() out of the loop
            removeClientFromList(client);
            client.Close();
        }
        private void receiveMessage(TcpClient client)
        {
            //---get the incoming data through a network stream---
            NetworkStream nwStream = client.GetStream();
            byte[] buffer = new byte[client.ReceiveBufferSize];

//            string dataReceived;
            //ToDo: split text send and recieve to a different function, before changing to receiving objects
            do
            {
                //---read incoming stream---
                try
                {
                    var dataReceived = _binaryFormatter.Deserialize(nwStream);
                    
                    //int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize); // ToRemove
                    //byte[] truncArray = new byte[bytesRead]; 
                    //Array.Copy(buffer, truncArray, truncArray.Length);

                    //---write back the text to the client---
                    Console.WriteLine("Sending to all clients ");
                    //_binaryFormatter.Serialize(nwStream, dataReceived);
                    SendToAllClients(dataReceived);
                    //SendToAllClients(truncArray);
                }
                catch
                {
                    break;
                }


            }
            while (true); // ToDo: find a way to change it - maybe to create searlization class
            //while (!dataReceived.ToLower().Contains("exit!"));
            //ToDo: to send & recieve repeatedly, should find a way to loop the send & receive 
            //      and take the client.close() out of the loop
            removeClientFromList(client);
            client.Close();
        }
    }
}
