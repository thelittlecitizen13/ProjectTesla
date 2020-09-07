using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TeslaServer
{
    public class TeslaServer
    {
        private IPAddress _localAddress;
        private TcpListener _server;
        private ConcurrentDictionary<string, TcpClient> _clientsList;

        public TeslaServer(int port)
        {
            _localAddress = IPAddress.Parse("127.0.0.1");
            _server = new TcpListener(_localAddress, port);
            _clientsList = new ConcurrentDictionary<string, TcpClient>();
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
                SendToAllClients($"{clientName} left!");
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
    }
}
