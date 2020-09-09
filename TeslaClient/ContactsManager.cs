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
        public IMemberData GetContactByName(string name)
        {
            if (ContactsDB.UsersList.ContainsKey(name))
                return ContactsDB.UsersList[name];
            if (ContactsDB.GroupsList.ContainsKey(name))
                return ContactsDB.GroupsList[name];
            return null;
        }
        public void UpdateGroup(GroupUpdateMessage message)
        {
            GroupData groupChanged = message.GroupChanged;
            switch(message.typeOfChange)
            {
                case ChangeType.Create:
                    ContactsDB.GroupsList.Add(groupChanged.Name, groupChanged);
                    break;
                case ChangeType.Update:
                    ContactsDB.GroupsList[groupChanged.Name] = groupChanged;
                    break;
                case ChangeType.Leave:
                    ContactsDB.RemoveGroup(groupChanged);
                    break;
                case ChangeType.Delete:
                    ContactsDB.RemoveGroup(groupChanged);
                    break;
                default:
                    break;
            }
        }
    }
}
