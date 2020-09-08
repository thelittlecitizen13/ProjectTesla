using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using TeslaCommon;

namespace TeslaServer
{
    public class Members
    {
        public ConcurrentDictionary<string, User> TeslaUsers { get; set; }
        public ConcurrentDictionary<string, Group> TeslaGroups { get; set; }
        public Members()
        {
            TeslaUsers = new ConcurrentDictionary<string, User>();
            TeslaGroups = new ConcurrentDictionary<string, Group>();
        }
        public bool AddUser(User member)
        {
            return TeslaUsers.TryAdd(member.Name, member);
        }
        public User RemoveUser(string memberName)
        {
            if (TeslaUsers.ContainsKey(memberName))
            {
                User deletedMember;
                TeslaUsers.TryRemove(memberName, out deletedMember);
                return deletedMember;
            }
            return null;
        }
        public User RemoveUser(TcpClient client)
        {
            foreach (var clt in TeslaUsers)
            {
                if (clt.Value.client == client)
                {
                    User deletedMember;
                    TeslaUsers.TryRemove(clt.Key, out deletedMember);
                    return deletedMember;
                }
            }
            return null;
        }
        public bool AddGroup(Group group)
        {
            return TeslaGroups.TryAdd(group.Name, group);
        }
        public Group RemoveGroup(string memberName)
        {
            if (TeslaGroups.ContainsKey(memberName))
            {
                Group deletedMember;
                TeslaGroups.TryRemove(memberName, out deletedMember);
                return deletedMember;
            }
            return null;
        }
        

    }
}
