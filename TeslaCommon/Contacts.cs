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
        }
        public void AddContact(MemberData memberData)
        {
            if (!ContactList.ContainsKey(memberData.MemberName))
            {
                ContactList.Add(memberData.MemberName, memberData);
            }
        }
        public void RemoveContact(MemberData memberData)
        {
            if (ContactList.ContainsKey(memberData.MemberName))
            {
                ContactList.Remove(memberData.MemberName);
            }
        }
    }
}
