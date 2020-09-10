using System;
using System.Collections.Generic;
using System.Linq;
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
            ContactsDB.GroupsList["Everyone"] = contacts.GroupsList["Everyone"];
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
        public IMemberData GetMemberByUID(string uid)
        {
            //var user = ContactsDB.UsersList.Values.Where(usr => usr.UID == uid).ToList().FirstOrDefault();
            //if (user != null)
            //    return user;
            //var group = ContactsDB.GroupsList.Values.Where(usr => usr.UID == uid).ToList().FirstOrDefault();
            //if (group != null)
            //    return group;
            //return null;
            foreach (var user in ContactsDB.UsersList)
            {
                if (user.Value.UID == uid)
                    return user.Value;
            }
            foreach (var group in ContactsDB.GroupsList)
            {
                if (group.Value.UID == uid)
                    return group.Value;
            }
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
