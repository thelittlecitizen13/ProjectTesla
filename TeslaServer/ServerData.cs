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
        public ISerializer Serializer;
        public Members MembersDB;
        public Contacts ContactsDB;
        public UserData AdminData;
        public ServerData(ISerializer serializer, Members membersDB, Contacts contactsDB, UserData adminData)
        {
            Serializer = serializer;
            MembersDB = membersDB;
            ContactsDB = contactsDB;
            AdminData = adminData;
        }
    }
}
