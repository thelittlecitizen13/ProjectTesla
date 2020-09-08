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
        public bool IsSendPicture(string input)
        {
            return input.ToLower() == "picture" || input.ToLower().StartsWith("picture;");
        }
        public bool IsSendScreenShot(string input)
        {
            return input.ToLower() == "picture";
        }
        public bool IsSendLocalPicture(string input)
        {
            return input.ToLower().StartsWith("picture;");
        }
        public bool IsFileExists(string filePath)
        {
            return (File.Exists(filePath));
        }
        public string ValidateContactChoose(Contacts contacts)
        {
            string choose = Console.ReadLine();
            while(!contacts.ContactList.ContainsKey(choose) || choose.ToLower() != "exit")
            {
                _outputManager.DisplayText("Contact not found. Please try again");
                choose = Console.ReadLine();
            }
            return choose;
        }
    }
}
