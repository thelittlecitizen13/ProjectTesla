using System.Net.Sockets;

namespace TeslaCommon
{
    public class User : IMember
    {
        public string Name { get; set; }
        public IMemberData Data { get; set; }
        public TcpClient client { get; set; }
        public NetworkStream nwStream { get; set; }
        public User(UserData memberData, TcpClient tcpClient)
        {
            Name = memberData.Name;
            Data = memberData;
            client = tcpClient;
            nwStream = client.GetStream();
        }
    }
}
