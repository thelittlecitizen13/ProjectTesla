using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using TeslaCommon;

namespace TeslaClient
{
    public class MessageReceiver
    {
        private ISerializer _serializer;
        private IMemberData _currentChatMember;
        private ClientData _clientData;
        private object _currentMemberLocker = new object();
        public bool IsNotifyUnSeenMessages;
        public ChatHistories LocalChatHistories { get; set; }
        
        public MessageReceiver(ClientData clientData)
        {
            _clientData = clientData;
            _serializer = new BinarySerializer();
            LocalChatHistories = new ChatHistories();
            IsNotifyUnSeenMessages = true;
        }

        private void processMessage(IMessage msg)
        {
            if (msg is TextMessage)
            {
                if (GetCurrentChatMember() != null && msg.Source.Equals(GetCurrentChatMember()))
                    processTextMessage((TextMessage)msg);
                else
                    LocalChatHistories.AddToChatHistory(msg);
                return;
            }
            if (msg is GroupMessage)
            {
                if (msg.Source.Equals(GetCurrentChatMember()))
                    processGroupMessage((GroupMessage)msg);
                else
                    LocalChatHistories.AddToChatHistory(msg);
                return;
            }
            if (msg is GroupUpdateMessage)
            {
                processGroupUpdateMessage((GroupUpdateMessage)msg);
                return;
            }
            if (msg is ImageMessage)
            {
                if (msg.Source.Equals(GetCurrentChatMember()))
                    processImageMessage((ImageMessage)msg);
                else
                    LocalChatHistories.AddToChatHistory(msg);
                return;
            }
            if (msg is ContactsMessage)
            {
                processContactsMessage((ContactsMessage)msg);
                return;
            }
            _clientData.Outputter.DisplayText("Fatal - Message display failed"); // for Debug
        }
        private void processTextMessage(TextMessage msg)
        {
            string textToShow = $"{msg.MessageTime.ToString()} - {msg.Source.Name}: {msg.Message}";
            _clientData.Outputter.DisplayText(textToShow);
        }
        private void processGroupMessage(GroupMessage msg)
        {
            string textToShow = $"{msg.MessageTime.ToString()} - {msg.Author.Name}: {msg.Message}";
            _clientData.Outputter.DisplayText(textToShow);
        }
        private void processGroupUpdateMessage(GroupUpdateMessage msg)
        {
            _clientData.contactsManager.UpdateGroup(msg);
            _clientData.contactsManager.UpdateContactsDB();
        }
        private void processImageMessage(ImageMessage msg)
        {
            string textToShow = $"{msg.MessageTime.ToString()} - {msg.Source.Name}: Image Received";
            _clientData.Outputter.DisplayText(textToShow);
            string imgPath = _clientData.Outputter.SaveAnImage(msg.Image);
            _clientData.Outputter.DisplayAnImage(imgPath);
        }
        private void processContactsMessage(ContactsMessage contactsMessage)
        {
            _clientData.contactsManager.UpdateContactsDB(contactsMessage.ContactList);
        }
        public IMessage ReceiveAMessage()
        {
            try
            {
                var dataReceived = _serializer.Deserialize(_clientData.NwStream);
                return (IMessage)dataReceived;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message); // Debug
                return null;
            }

        }

        public void Run()
        {
            IMessage msg = ReceiveAMessage();
            if (msg == null)
                return;
            processMessage(msg);
        }
        public void ShowUnSeenMessages()
        {
            IMessage oldMessage = null;
            do
            {
                oldMessage = LocalChatHistories.GetLastMessage(GetCurrentChatMember());
                if (oldMessage != null)
                    processMessage(oldMessage);
            }
            while (oldMessage != null);
        }

        public IMemberData GetCurrentChatMember()
        {
            lock(_currentMemberLocker)
            {
                return _currentChatMember;
            }   
        }
        public void SetCurrentMemberChat(IMemberData currentMember)
        {
            lock (_currentMemberLocker)
            {
                _currentChatMember = currentMember;
            }
        }
        public void NotifyForUnSeenMessages()
        {
            if (!IsNotifyUnSeenMessages)
                return;
            //Dictionary<string, int> UnseenMessagesDB = new Dictionary<string, int>()
            StringBuilder sb = new StringBuilder();
            foreach (var unseenFromAMember in LocalChatHistories.HistoryStatus)
            {
                string memberUID = unseenFromAMember.Key;
                IMemberData member = _clientData.contactsManager.GetMemberByUID(memberUID);
                if (member != null && member.Name != "Server")
                    sb.AppendLine($"You have {unseenFromAMember.Value} messages from {member.Name}");
            }
            if (!string.IsNullOrWhiteSpace(sb.ToString()))
                _clientData.Outputter.DisplayText(sb.ToString(),ConsoleColor.Gray);
        }
    }

}
