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
        private void displayTextToConsole(string text)
        {
            Console.WriteLine(text);
        }
        public void DisplayText(string text)
        {
            // You can control where the text will be displayed
            displayTextToConsole(text);
        }
        public void DisplayAnImage(string imgPath)
        {
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
            string path = @"C:\images\" + _clientName;
            Directory.CreateDirectory(path);
            return path;
        }
    }
}
