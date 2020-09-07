using System;
namespace TeslaClient
{
    public class InputManager
    {
        public string GetUserInput(string textToDisplay)
        {
            Console.WriteLine(textToDisplay);
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
    }
}
