using System;
using System.Collections.Generic;
using System.Text;

namespace TeslaCommon
{
    [Serializable]
    public class Contacts
    {
        public Dictionary<string, MemberData> ContactList { get; set; }
        public Contacts()
        {
            ContactList = new Dictionary<string, MemberData>();
            ContactList.TryAdd("Everyone", new MemberData("Everyone"));
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
                ContactList.Remove(memberData.MemberName, out removedMemberData);
            }
        }
    }
}
