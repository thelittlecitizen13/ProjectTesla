using System;

namespace TeslaCommon
{
    [Serializable]
    public class ContactsMessage : IMessage
    {
        public IMemberData Source { get; set; }
        public IMemberData Destination { get; set; }
        public DateTime MessageTime { get; set; }
        public Contacts ContactList { get; set; }
        public ContactsMessage(Contacts contactsList, UserData src, UserData dst)
        {
            Source = src;
            Destination = dst;
            ContactList = contactsList;
            MessageTime = DateTime.Now;
        }
    }
}
