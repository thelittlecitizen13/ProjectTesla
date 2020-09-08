using System;

namespace TeslaCommon
{
    [Serializable]
    public class ContactsMessage : IMessage
    {
        public MemberData Source { get; set; }
        public MemberData Destination { get; set; }
        public DateTime MessageTime { get; set; }
        public Contacts ContactList { get; set; }
        public ContactsMessage(Contacts contactsList, MemberData src, MemberData dst)
        {
            Source = src;
            Destination = dst;
            ContactList = contactsList;
            MessageTime = DateTime.Now;
        }
    }
}
