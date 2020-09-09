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
            ContactsDB.UsersList = contacts.UsersList;
            ContactsMenu.CreateMenu(ContactsDB);
        }
        public void UpdateContactsDB()
        {
            ContactsMenu.CreateMenu(ContactsDB);
        }
        public UserData GetContactByName(string name)
        {
            if (ContactsDB.UsersList.ContainsKey(name))
                return ContactsDB.UsersList[name];
            return null;
        }
    }
}
