using System;
using System.Collections.Generic;
using System.Text;
using TeslaCommon;

namespace TeslaClient
{
    public class ContactsManager
    {
        public Contacts ContactsDB;
        public ContactsMenu ContactsMenu;

        public ContactsManager()
        {
            ContactsDB = new Contacts();
            ContactsMenu = new ContactsMenu();
        }
        public void UpdateContactsDB(Contacts contacts)
        {
            ContactsDB.ContactList = contacts.ContactList;
            ContactsMenu.CreateMenu(ContactsDB);
        }
        public void UpdateContactsDB()
        {
            ContactsMenu.CreateMenu(ContactsDB);
        }
        public MemberData GetContactByName(string name)
        {
            if (ContactsDB.ContactList.ContainsKey(name))
                return ContactsDB.ContactList[name];
            return null;
        }
    }
}
