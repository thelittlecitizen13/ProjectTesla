﻿using System;
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
            foreach (var contact in contacts.ContactList.Keys)
            {
                sb.AppendLine(contact);
            }
            sb.AppendLine("Enter contact name to start a chat with");
            Menu = sb.ToString();
        }
    }
}