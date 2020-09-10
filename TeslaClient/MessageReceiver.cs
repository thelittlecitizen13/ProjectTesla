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
        private NetworkStream _nwStream;
        private ISerializer _serializer;
        private OutputManager _outputManager;
        private ContactsManager _contactsManager;
        private IMemberData _currentChatMember;
        private object _currentMemberLocker = new object();
        public bool IsNotifyUnSeenMessages;
        public ChatHistories LocalChatHistories { get; set; }

        public MessageReceiver(NetworkStream networkStream, OutputManager outputManager, ContactsManager contactsManager)
        {
            _nwStream = networkStream;
            _serializer = new BinarySerializer();
            _outputManager = outputManager;
            _contactsManager = contactsManager;
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
            _outputManager.DisplayText("Fatal - Message display failed"); // for Debug
        }
        private void processTextMessage(TextMessage msg)
        {
            string textToShow = $"{msg.MessageTime.ToString()} - {msg.Source.Name}: {msg.Message}";
            _outputManager.DisplayText(textToShow);
        }
        private void processGroupMessage(GroupMessage msg)
        {
            string textToShow = $"{msg.MessageTime.ToString()} - {msg.Author.Name}: {msg.Message}";
            _outputManager.DisplayText(textToShow);
        }
        private void processGroupUpdateMessage(GroupUpdateMessage msg)
        {            
            _contactsManager.UpdateGroup(msg);
            _contactsManager.UpdateContactsDB();
        }
        private void processImageMessage(ImageMessage msg)
        {
            string textToShow = $"{msg.MessageTime.ToString()} - {msg.Source.Name}: Image Received";
            _outputManager.DisplayText(textToShow);
            string imgPath = _outputManager.SaveAnImage(msg.Image);
            _outputManager.DisplayAnImage(imgPath);
        }
        private void processContactsMessage(ContactsMessage contactsMessage)
        {
            _contactsManager.UpdateContactsDB(contactsMessage.ContactList);
        }
        public IMessage ReceiveAMessage()
        {
            try
            {
                var dataReceived = _serializer.Deserialize(_nwStream);
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
            Console.WriteLine(msg.GetType()); //debug
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
                IMemberData member = _contactsManager.GetMemberByUID(memberUID);
                if (member != null && member.Name != "Server")
                    sb.AppendLine($"You have {unseenFromAMember.Value} messages from {member.Name}");
            }
            if (!string.IsNullOrWhiteSpace(sb.ToString()))
                _outputManager.DisplayText(sb.ToString(),ConsoleColor.Gray);
        }
    }

}
