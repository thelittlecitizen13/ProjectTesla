using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TeslaCommon;

namespace TeslaClient
{
    public class TeslaClient
    {
        private const string EXIT_COMMAND = "/exit";
        private IPAddress _serverAddress;
        private int _port;
        private TcpClient _client;
        private MessageSender _messageSender;
        private MessageReceiver _messageReceiver;
        private OutputManager _outputManager;
        private InputManager _inputManager;
        private bool _chatRoomExitToken;
        public ContactsManager ContactsMan;
        public string Name;
        public UserData ClientData;
        private CommandManager _commandManager;

        public TeslaClient(string address, int port, int clientNumber)
        {
            _serverAddress = IPAddress.Parse(address);
            _port = port;
            Name = "client" + clientNumber;
            _client = new TcpClient(_serverAddress.ToString(), _port);
            _outputManager = new OutputManager(Name);
            _inputManager = new InputManager(_outputManager);
            ContactsMan = new ContactsManager();
            _commandManager = new CommandManager(this);
            _messageReceiver = new MessageReceiver(_client.GetStream(), _outputManager, ContactsMan);
            _messageSender = new MessageSender(_client.GetStream(), _outputManager, _inputManager, Name, _commandManager);
            ClientData = new UserData(Name);
            _chatRoomExitToken = false;
            
        }

        private void WriteAMessage(IMemberData currentChatMember)
        {
            // ToDo: print you are now in a chat room with..
            _outputManager.DisplayText("Enter your message");
            string msg = _inputManager.GetUserInput();
            if (msg.ToLower() == EXIT_COMMAND)
            {
                _chatRoomExitToken = true;
                return;
            }
            if (msg.StartsWith("/"))
            {
                _messageSender.HandleUserCommands(msg);
                return;
            }
            if (currentChatMember.GetType() == typeof(GroupData))
                _messageSender.SendNewMessage(msg, currentChatMember, currentChatMember, ClientData);
            else
                _messageSender.SendNewMessage(msg, currentChatMember, ClientData, ClientData);
        }
        
        private void ReceiveMessages()
        {
            _messageReceiver.Run();
        }
        
        private void registerToServerWithMessage(NetworkStream nwStream)
        {
            //IMemberData EveryOneGroup = _contactsManager.GetContactByName("Everyone");
            _messageSender.SendNewMessage(Name, new UserData("Server") , ClientData, ClientData); 
            TextMessage serverAnswer = (TextMessage)_messageReceiver.ReceiveAMessage();
            Console.WriteLine(serverAnswer.Message);
            ContactsMessage contactsMessage = (ContactsMessage)_messageReceiver.ReceiveAMessage(); 
            ContactsMan.UpdateContactsDB(contactsMessage.ContactList);
        }
        private void displayContactMenu()
        {
            _outputManager.DisplayText(ContactsMan.ContactsMenu.Menu);
        }
        public void Run()
        {
            ContactsMan.UpdateContactsDB();
            try
            {
                using (NetworkStream nwStream = _client.GetStream())
                {
                    registerToServerWithMessage(nwStream);
                    ThreadPool.QueueUserWorkItem(obj =>
                    {
                        while (true)
                        {
                            ReceiveMessages();
                        }
                    });
                    while (true)
                    {
                        _chatRoomExitToken = false;
                        displayContactMenu();
                        string choice = _inputManager.ValidateContactChoose(ContactsMan);
                        if (choice.ToLower() == EXIT_COMMAND)
                            break;
                        IMemberData chatMember = ContactsMan.GetContactByName(choice);
                        while (!_chatRoomExitToken)
                        {
                            WriteAMessage(chatMember);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                _client.Close();
                Console.ReadLine();
            }
        }
        
    }
}
