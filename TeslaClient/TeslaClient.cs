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
        public string Name;
        private MessageSender _messageSender;
        private MessageReceiver _messageReceiver;
        private OutputManager _outputManager;
        private InputManager _inputManager;
        private MemberData _clientData;
        private ContactsManager _contactsManager;
        private bool _chatRoomExitToken;

        public TeslaClient(string address, int port, int clientNumber)
        {
            _serverAddress = IPAddress.Parse(address);
            _port = port;
            Name = "client" + clientNumber;
            _client = new TcpClient(_serverAddress.ToString(), _port);
            _outputManager = new OutputManager(Name);
            _inputManager = new InputManager(_outputManager);
            _contactsManager = new ContactsManager();
            _messageReceiver = new MessageReceiver(_client.GetStream(), _outputManager, _contactsManager);
            _messageSender = new MessageSender(_client.GetStream(), _outputManager, _inputManager, Name);
            _clientData = new MemberData(Name);
            _chatRoomExitToken = false;
            
        }

        private void WriteAMessage(MemberData currentChatMember)
        {
            _outputManager.DisplayText("Enter your message");
            string msg = _inputManager.GetUserInput();
            if (msg.ToLower() != EXIT_COMMAND)
                _messageSender.SendNewMessage(msg, currentChatMember, _clientData);
            else
                _chatRoomExitToken = true;
        }
        private void ReceiveMessages(MemberData currentChatMember)
        {
            _messageReceiver.Run();
        }
        
        private void registerToServerWithMessage(NetworkStream nwStream)
        {
            _messageSender.SendNewTextMessage(Name, _clientData, new MemberData("all"));
            TextMessage serverAnswer = (TextMessage)_messageReceiver.ReceiveAMessage();
            Console.WriteLine(serverAnswer.Message);
        }
        private void displayContactMenu()
        {
            _outputManager.DisplayText(_contactsManager.ContactsMenu.Menu);
        }
        public void Run()
        {
            _contactsManager.UpdateContactsDB();
            //bool _menuExitToken = false;
            try
            {
                using (NetworkStream nwStream = _client.GetStream())
                {
                    registerToServerWithMessage(nwStream);
                    while (true)
                    {
                        displayContactMenu();
                        string choice = _inputManager.ValidateContactChoose(_contactsManager.ContactsDB);
                        if (choice.ToLower() == EXIT_COMMAND)
                            break;
                        MemberData chatMember = _contactsManager.GetContactByName(choice);
                        //ToDo: Access private chat rooms from here
                        ThreadPool.QueueUserWorkItem(obj =>
                        {
                            while (!_chatRoomExitToken)
                            {
                                ReceiveMessages(nwStream, chatMember);
                            }
                        });
                        while (!_chatRoomExitToken)
                        {
                            WriteAMessage(nwStream, chatMember);
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
            }
        }
        
        private void registerToServer(NetworkStream nwStream) //Deprecated
        {

            byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(Name);
            nwStream.Write(bytesToSend, 0, bytesToSend.Length);
            byte[] bytesToRead = new byte[_client.ReceiveBufferSize];
            int bytesRead = nwStream.Read(bytesToRead, 0, _client.ReceiveBufferSize);
            Console.WriteLine(Encoding.ASCII.GetString(bytesToRead, 0, bytesRead));
        }
    }
}
