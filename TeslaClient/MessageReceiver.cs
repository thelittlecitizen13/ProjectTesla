using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using TeslaCommon;

namespace TeslaClient
{
    public class MessageReceiver
    {
        private NetworkStream _nwStream;
        private IFormatter _binaryFormatter;
        private OutputManager _outputManager;
        private ContactsManager _contactsManager;

        public MessageReceiver(NetworkStream networkStream, OutputManager outputManager, ContactsManager contactsManager)
        {
            _nwStream = networkStream;
            _binaryFormatter = new BinaryFormatter();
            _outputManager = outputManager;
            _contactsManager = contactsManager;
        }

        private void processMessage(IMessage msg)
        {
            if (msg is TextMessage)
            {
                processTextMessage((TextMessage)msg);
                return;
            }
            if (msg is ImageMessage)
            {
                processImageMessage((ImageMessage)msg);
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
            string textToShow = $"{msg.MessageTime.ToString()} - {msg.Source.MemberName}: {msg.Message}";
            _outputManager.DisplayText(textToShow);
        }

        private void processImageMessage(ImageMessage msg)
        {
            string textToShow = $"{msg.MessageTime.ToString()} - {msg.Source.MemberName}: Image Received";
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
                _binaryFormatter = new BinaryFormatter();
                var dataReceived = _binaryFormatter.Deserialize(_nwStream);
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


    }

}
