using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using TeslaCommon;

namespace TeslaClient
{
    public class ClientData
    {
        public NetworkStream NwStream;
        public OutputManager Outputter;
        public InputManager Inputter;
        public CommandManager commandManager;
        public ContactsManager contactsManager;
        public ClientData(NetworkStream networkStream, OutputManager outputManager, 
            InputManager inputManager, CommandManager commandManager, ContactsManager contactsManager)
        {
            NwStream = networkStream;
            Outputter = outputManager;
            Inputter = inputManager;
            this.commandManager = commandManager;
            this.contactsManager = contactsManager;
        }
    }
}
