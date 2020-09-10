using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;


namespace TeslaClient
{
    public class OutputManager
    {
        private string _clientName;
        public OutputManager(string clientName)
        {
            _clientName = clientName;
        }
        public void ClearScreen()
        {
            Console.Clear();
        }
        private void displayTextToConsole(string text, ConsoleColor consoleColor = ConsoleColor.White)
        {
            Console.ForegroundColor = consoleColor;
            Console.WriteLine(text);
            Console.ResetColor();
        }
        public void DisplayText(string text)
        {
            // You can control where the text will be displayed
            displayTextToConsole(text);
        }
        public void DisplayText(string text, ConsoleColor consoleColor)
        {
            // You can control where the text will be displayed
            displayTextToConsole(text, consoleColor);
        }
        public void DisplayAnImage(string imgPath)
        {
            // relevant for windows clients only
            try
            {
                Process.Start("cmd", $"/c {imgPath}");
            }
            catch
            {
                displayTextToConsole("Cannot open image");
            }
            
        }
        public string SaveAnImage(Bitmap img)
        {
            string folderPath = CreateImageFolder();
            string imgPath = folderPath + "clientPrintScreen" + Guid.NewGuid() + ".jpg";
            img.Save(imgPath,ImageFormat.Jpeg);
            return imgPath;
        }
        public string CreateImageFolder()
        {
            string path = @"C:\TeslaImages\" + _clientName;
            Directory.CreateDirectory(path);
            return path;
        }

    }
}
