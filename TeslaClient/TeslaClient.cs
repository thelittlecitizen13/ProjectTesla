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
                _messageSender.HandleUserCommands(msg, ClientData, currentChatMember);
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
            _outputManager.DisplayText(_commandManager.GetBasicCommandsHelp());
        }
        public void Run()
        {
            ContactsMan.UpdateContactsDB();
            NetworkStream nwStream = _client.GetStream();
            try
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
                    resetChat();
                    string choice = _inputManager.ValidateContactChoose(ContactsMan);
                    if (choice.ToLower() == EXIT_COMMAND)
                        break;
                    if (choice.StartsWith("/"))
                    {
                        processCommand(choice);
                        continue;
                    }
                    startNewConversation(choice);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                nwStream.Close();
                _client.Close();
                Console.ReadLine();
            }
        }
        private void startNewConversation(string memberName)
        {
            IMemberData chatMember = ContactsMan.GetContactByName(memberName);
            _messageReceiver.SetCurrentMemberChat(chatMember);
            _outputManager.ClearScreen();
            _outputManager.DisplayText($"You are now in a chat with {chatMember.Name}!", ConsoleColor.Green);
            _outputManager.DisplayText(_commandManager.GetChatCommands(), ConsoleColor.Cyan);
            _messageReceiver.ShowUnSeenMessages();
            while (!_chatRoomExitToken)
            {
                WriteAMessage(chatMember);
            }
        }
        private void resetChat()
        {
            _outputManager.ClearScreen();
            _chatRoomExitToken = false;
            displayContactMenu();
            IMemberData chatMember = ContactsMan.GetContactByName("Admin");
            _messageReceiver.SetCurrentMemberChat(chatMember);
            _messageReceiver.ShowUnSeenMessages();
        }
        public OutputManager GetOutputManager()
        {
            return _outputManager;
        }
        private void processCommand(string command)
        {
            if (command.ToLower() == "/help")
            {
                _outputManager.DisplayText(_commandManager.GetCommandHelp());
                return;
            }
            if (command.ToLower() == "/refresh")
            {
                return;
            }
            if (command.ToLower() == "/notifications")
            {
                if (_messageReceiver.IsNotifyUnSeenMessages)
                    _messageReceiver.IsNotifyUnSeenMessages = false;
                else
                    _messageReceiver.IsNotifyUnSeenMessages = true;
                return;
            }
            if (command.StartsWith("/"))
            {
                _messageSender.HandleUserCommands(command, ClientData, ClientData);
                _outputManager.DisplayText("Press any key to continue..", ConsoleColor.Gray);
                _inputManager.ReadLine();
            }
           
        }
        
    }
}
