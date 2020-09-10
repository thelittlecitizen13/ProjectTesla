using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
                _messageReceiver.SetCurrentMemberChat(null);
                return;
            }
            if(msg.ToLower() == "/help")
            {
                _outputManager.DisplayText(_commandManager.GetCommandHelp());
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
            _outputManager.DisplayText("Type /help to see available commands (like group controlling)");
            _outputManager.DisplayText("Type /refresh to refresh contacts");
            _outputManager.DisplayText("Type /exit to exit");
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
                        try 
                        {
                            while (true)
                            {
                                ReceiveMessages();
                            }
                        }
                        catch (Exception e)
                        {
                            _outputManager.DisplayText(e.Message);
                        }
                        
                    });
                    ThreadPool.QueueUserWorkItem(obj =>
                    {
                        try
                        {
                            while (true)
                            {
                                _messageReceiver.NotifyForUnSeenMessages();
                                Task.Delay(10000).Wait();
                            }
                        }
                        catch (Exception e)
                        {
                            _outputManager.DisplayText(e.Message);
                        }

                    });

                    while (true)
                    {
                        _chatRoomExitToken = false;
                        displayContactMenu();
                        IMemberData chatMember = ContactsMan.GetContactByName("Admin");
                        _messageReceiver.SetCurrentMemberChat(chatMember);
                        _messageReceiver.ShowUnSeenMessages();
                        string choice = _inputManager.ValidateContactChoose(ContactsMan);
                        if (choice.ToLower() == EXIT_COMMAND)
                            break;
                        if (choice.ToLower() == "/help")
                        {
                            _outputManager.DisplayText(_commandManager.GetCommandHelp());
                            continue;
                        }
                        if (choice.ToLower() == "/refresh")
                        {
                            continue;
                        }
                        if (choice.StartsWith("/"))
                        {
                            _messageSender.HandleUserCommands(choice);
                            continue;
                        }
                         chatMember = ContactsMan.GetContactByName(choice);
                        _messageReceiver.SetCurrentMemberChat(chatMember);
                        _messageReceiver.ShowUnSeenMessages();
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
