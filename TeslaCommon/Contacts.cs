using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace TeslaCommon
{
    [Serializable]
    public class Contacts
    {
        public ConcurrentDictionary<string, MemberData> ContactList { get; set; }
        public Contacts()
        {
            ContactList = new ConcurrentDictionary<string, MemberData>();
        }
        public bool AddContact(MemberData memberData)
        {
            return ContactList.TryAdd(memberData.MemberName, memberData);
        }
        public void RemoveContact(MemberData memberData)
        {
            if (ContactList.ContainsKey(memberData.MemberName))
            {
                MemberData removedMemberData;
                ContactList.TryRemove(memberData.MemberName, out removedMemberData);
            }
        }
    }
}
