using System;
using System.Collections.Generic;
using System.Text;

namespace TeslaCommon
{
    [Serializable]
    public class Contacts
    {
        public Dictionary<string, UserData> ContactList { get; set; }
        public Contacts()
        {
            ContactList = new Dictionary<string, UserData>();
            ContactList.TryAdd("Everyone", new UserData("Everyone"));
        }
        public bool AddContact(UserData memberData)
        {
            return ContactList.TryAdd(memberData.Name, memberData);
        }
        public void RemoveContact(UserData memberData)
        {
            if (ContactList.ContainsKey(memberData.Name))
            {
                UserData removedMemberData;
                ContactList.Remove(memberData.Name, out removedMemberData);
            }
        }
    }
}
