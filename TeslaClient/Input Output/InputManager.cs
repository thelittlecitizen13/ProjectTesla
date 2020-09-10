using System;
using System.IO;
using TeslaCommon;

namespace TeslaClient
{
    public class InputManager
    {
        private OutputManager _outputManager;
        public InputManager(OutputManager outputManager)
        {
            _outputManager = outputManager;
        }
        public string GetUserInput()
        {
            return Console.ReadLine();
        }
        public bool IsFileExists(string filePath)
        {
            return (File.Exists(filePath));
        }
        public string ValidateContactChoose(ContactsManager contactsManager)
        {
            string choose = Console.ReadLine();

            while(choose.StartsWith("/") || contactsManager.GetContactByName(choose) == null)
            {
                if (choose.StartsWith("/"))
                    return choose;
                _outputManager.DisplayText("Contact not found. Please try again", ConsoleColor.Red);
                choose = Console.ReadLine();
            }
            return choose;
        }
        public void ReadLine()
        {
            Console.ReadLine();
        }
    }
}
