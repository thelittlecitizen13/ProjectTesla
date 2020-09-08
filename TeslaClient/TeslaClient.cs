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
        private IPAddress _serverAddress;
        private int _port;
        private TcpClient _client;
        public string Name;
        private MessageSender _messageSender;
        private MessageReceiver _messageReceiver;
        private OutputManager _outputManager;
        private InputManager _inputManager;
        private MemberData _clientData;
        private Contacts _contactsDB;
        private ContactsMenu _contactsMenu;

        public TeslaClient(string address, int port, int clientNumber)
        {
            _serverAddress = IPAddress.Parse(address);
            _port = port;
            Name = "client" + clientNumber;
            _client = new TcpClient(_serverAddress.ToString(), _port);
            _outputManager = new OutputManager(Name);
            _inputManager = new InputManager(_outputManager);
            _messageReceiver = new MessageReceiver(_client.GetStream(), _outputManager);
            _messageSender = new MessageSender(_client.GetStream(), _outputManager, _inputManager, Name);
            _clientData = new MemberData(Name);
            _contactsDB = new Contacts();
            _contactsMenu = new ContactsMenu();
        }

        private void WriteAMessage(NetworkStream nwStream)
        {
            _messageSender.SendNewMessage();
        }
        private void ReceiveMessages(NetworkStream nwStream)
        {
            _messageReceiver.Run();
        }
        
        private void registerToServerWithMessage(NetworkStream nwStream)
        {
            _messageSender.SendNewTextMessage(Name, _clientData, new MemberData("all"));
            TextMessage serverAnswer = (TextMessage)_messageReceiver.ReceiveAMessage();
            Console.WriteLine(serverAnswer.Message);
        }
        public void Run()
        {
            updateContactsDB(_contactsDB);
            try
            {
                using (NetworkStream nwStream = _client.GetStream())
                {
                    registerToServerWithMessage(nwStream);
                    while (true)
                    {
                        _outputManager.DisplayText(_contactsMenu.Menu);
                        //ToDo: Access private chat rooms from here
                        ThreadPool.QueueUserWorkItem(obj => ReceiveMessages(nwStream));
                        while (true)
                        {
                            WriteAMessage(nwStream);
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
        private void updateContactsDB(Contacts contacts)
        {
            _contactsDB.ContactList = contacts.ContactList;
            _contactsMenu.CreateMenu(_contactsDB);

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
