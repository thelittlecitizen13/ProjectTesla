using System.Net;
using System.Net.Sockets;

namespace TeslaClient
{
    public class TeslaClient
    {
        private IPAddress _serverAddress;
        private int _port;
        private TcpClient _client;
        public string Name;

        public TeslaClient(string address, int port, int clientNumber)
        {
            _serverAddress = IPAddress.Parse(address);
            _port = port;
            Name = "client" + clientNumber;
            _client = new TcpClient(_serverAddress.ToString(), _port);
        }
    }
}
