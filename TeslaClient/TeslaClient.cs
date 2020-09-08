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
        }

        private void WriteAMessage(NetworkStream nwStream)
        {
            _messageSender.SendNewMessage();
            //Console.WriteLine("Your message:");
            //string msg = _inputManager.GetUserInput();
            //string textToSend = $"{Name}: {msg}";
            //byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(textToSend);

            ////---send the text---
            ////Console.WriteLine("Sending : " + msg);
            //nwStream.Write(bytesToSend, 0, bytesToSend.Length);
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
            try
            {
                using (NetworkStream nwStream = _client.GetStream())
                {
                    registerToServerWithMessage(nwStream);
                    while (true)
                    {

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
