using System.Net.Sockets;

namespace TeslaCommon
{
    public class User : IMember
    {
        public string Name { get; set; }
        public MemberData Data { get; set; }
        public TcpClient client { get; set; }
        public NetworkStream nwStream { get; set; }
        public User(MemberData memberData, TcpClient tcpClient)
        {
            Name = memberData.MemberName;
            Data = memberData;
            client = tcpClient;
            nwStream = client.GetStream();
        }
    }
}
