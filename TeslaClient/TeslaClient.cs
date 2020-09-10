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
        private IPAddress _serverAddress;
        private TcpClient _client;
        private MessageSender _messageSender;
        private MessageReceiver _messageReceiver;
        private bool _chatRoomExitToken;
        public string Name;
        public UserData MyData;
        public ClientData clientData;
        

        public TeslaClient(string address, int port, int clientNumber)
        {
            _serverAddress = IPAddress.Parse(address);
            Name = "client" + clientNumber;
            _client = new TcpClient(_serverAddress.ToString(), port);
            clientDataCreator(_client.GetStream());
            _messageReceiver = new MessageReceiver(clientData);
            _messageSender = new MessageSender(clientData, Name);
            MyData = new UserData(Name);
            _chatRoomExitToken = false;
            
        }
        private void clientDataCreator(NetworkStream nwStream)
        {            
            OutputManager Outputter = new OutputManager(Name);
            InputManager Inputter = new InputManager(Outputter);
            CommandManager commandManager = new CommandManager(this);
            ContactsManager contactsManager = new ContactsManager();
            clientData = new ClientData(nwStream, Outputter, Inputter, commandManager, contactsManager);
        }
        private void WriteAMessage(IMemberData currentChatMember)
        {            
            string msg = clientData.Inputter.GetUserInput();
            if (msg.ToLower() == "/exit")
            {
                _chatRoomExitToken = true;
                _messageReceiver.SetCurrentMemberChat(null);
                return;
            }
            if(msg.ToLower() == "/help")
            {
                clientData.Outputter.DisplayText(clientData.commandManager.GetCommandHelp());
                return;
            }
            if (msg.StartsWith("/"))
            {
                _messageSender.HandleUserCommands(msg, MyData, currentChatMember);
                return;
            }
            if (currentChatMember.GetType() == typeof(GroupData))
                _messageSender.SendNewMessage(msg, currentChatMember, currentChatMember, MyData);
            else
                _messageSender.SendNewMessage(msg, currentChatMember, MyData, MyData);
        }
        
        private void ReceiveMessages()
        {
            _messageReceiver.Run();
        }
        private void registerToServerWithMessage(NetworkStream nwStream)
        {
            _messageSender.SendNewMessage(Name, new UserData("Server") , MyData, MyData); 
            TextMessage serverAnswer = (TextMessage)_messageReceiver.ReceiveAMessage();
            Console.WriteLine(serverAnswer.Message);
            ContactsMessage contactsMessage = (ContactsMessage)_messageReceiver.ReceiveAMessage();
            clientData.contactsManager.UpdateContactsDB(contactsMessage.ContactList);
        }
        private void displayContactMenu()
        {
            clientData.Outputter.DisplayText(clientData.contactsManager.ContactsMenu.Menu);
            clientData.Outputter.DisplayText(clientData.commandManager.GetBasicCommandsHelp());
        }
        public void Run()
        {
            clientData.contactsManager.UpdateContactsDB();
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
                        clientData.Outputter.DisplayText(e.Message);
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
                        clientData.Outputter.DisplayText(e.Message);
                    }

                });
                while (true)
                {
                    resetChat();
                    string choice = clientData.Inputter.ValidateContactChoose(clientData.contactsManager);
                    if (choice.ToLower() == "/exit")
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
            IMemberData chatMember = clientData.contactsManager.GetContactByName(memberName);
            _messageReceiver.SetCurrentMemberChat(chatMember);
            clientData.Outputter.ClearScreen();
            clientData.Outputter.DisplayText($"You are now in a chat with {chatMember.Name}!", ConsoleColor.Green);
            clientData.Outputter.DisplayText(clientData.commandManager.GetChatCommands(), ConsoleColor.Cyan);
            _messageReceiver.ShowUnSeenMessages();
            while (!_chatRoomExitToken)
            {
                WriteAMessage(chatMember);
            }
        }
        private void resetChat()
        {
            clientData.Outputter.ClearScreen();
            _chatRoomExitToken = false;
            clientData.Outputter.DisplayText($"Hey {Name}", ConsoleColor.Green);
            Task.Delay(100).Wait();
            displayContactMenu();
            IMemberData chatMember = clientData.contactsManager.GetContactByName("Admin");
            _messageReceiver.SetCurrentMemberChat(chatMember);
            _messageReceiver.ShowUnSeenMessages();
        }
        private void processCommand(string command)
        {
            if (command.ToLower() == "/help")
            {
                clientData.Outputter.DisplayText(clientData.commandManager.GetCommandHelp());
                return;
            }
            if (command.ToLower() == "/refresh")
                return;
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
                _messageSender.HandleUserCommands(command, MyData, MyData);
            }

        }
    }
}
