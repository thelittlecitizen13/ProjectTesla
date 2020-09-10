using System;
using System.Collections.Generic;
using System.Net.Sockets;
using TeslaCommon;
using System.Linq;

namespace TeslaServer
{
    public class MessageReceiver
    {
        private MessageSender _messageSender;
        private ServerData _serverDTO;
        private GroupUpdateProcessor _groupUpdateProcessor;
        public MessageReceiver(MessageSender messageSender, ServerData serverData)
        {
            _messageSender = messageSender;
            _serverDTO = serverData;
            _groupUpdateProcessor = new GroupUpdateProcessor(_messageSender, _serverDTO);
        }
        public void ReceiveMessage(TcpClient client)
        {
            NetworkStream nwStream = client.GetStream();
            do
            {
                try
                {
                    var dataReceived = _serverDTO.Serializer.Deserialize(nwStream);
                    processMessage((IMessage)dataReceived);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    break;
                }
            }
            while (true);
            removeUserFromMembersDB(client);
            client.Close();
        }
        private void processMessage(IMessage message)
        {
            if (message.GetType() == typeof(GroupUpdateMessage))
            {
                _groupUpdateProcessor.ProcessGroupUpdateMessage((GroupUpdateMessage)message);
                return;
            }
            if (message.GetType() == typeof(CommandMessage))
            {
                // ToDo: Handle command messages when implemented
                return;
            }
            _messageSender.DeliverMessageToDestination(message);
        }
        
        private void removeUserFromMembersDB(TcpClient client)
        {
            User removedUser = _serverDTO.MembersDB.RemoveUser(client);
            _serverDTO.ContactsDB.RemoveUser((UserData)removedUser.Data);
            if (removedUser != null)
            {
                TextMessage userLeftChatMessage = new TextMessage($"{removedUser.Name} has left the chat!", _serverDTO.AdminData, _serverDTO.AdminData);
                _messageSender.SendToAllClients(userLeftChatMessage);
                ContactsMessage contactsUpdateMessage = new ContactsMessage(_serverDTO.ContactsDB, _serverDTO.AdminData, _serverDTO.AdminData);
                _messageSender.SendToAllClients(contactsUpdateMessage); // ToDo: move to a function with indicative name
            }
        }
    }
}
