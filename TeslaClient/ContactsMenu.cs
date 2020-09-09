using System;
using System.Collections.Generic;
using System.Text;
using TeslaCommon;

namespace TeslaClient
{
    public class ContactsMenu
    {
        public string Menu { get; set; }
        public void CreateMenu(Contacts contacts)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("To whom would you like to send a message");
            foreach (var contact in contacts.UsersList.Keys)
            {
                sb.AppendLine($"User - {contact}");
            }
            foreach (var contact in contacts.GroupsList.Keys)
            {
                sb.AppendLine($"Group - {contact}");
            }
            sb.AppendLine("Enter contact name to start a chat with");
            sb.AppendLine("Type /exit to exit Tesla");
            Menu = sb.ToString();
        }
    }
}
