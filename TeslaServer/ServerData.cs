using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using TeslaCommon;

namespace TeslaServer
{
    public class ServerData
    {
        public IPAddress LocalAddress;
        public TcpListener Server;
        public IFormatter BinarySerializer;
        public Members MembersDB;
        public Contacts ContactsDB;
        public UserData AdminData;
        public ServerData(IPAddress iPAddress, TcpListener server, IFormatter formatter, Members membersDB, Contacts contactsDB, UserData adminData)
        {
            LocalAddress = iPAddress;
            Server = server;
            BinarySerializer = formatter;
            MembersDB = membersDB;
            ContactsDB = contactsDB;
            AdminData = adminData;
        }
    }
}
