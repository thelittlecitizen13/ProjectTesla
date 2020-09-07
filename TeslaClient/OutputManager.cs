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
        public void DisplayAnImage(string imgPath)
        {
            try
            {
                Process.Start("cmd", $"/c {imgPath}");
            }
            catch
            {
                System.Console.WriteLine("Cannot open image");
            }
            
        }
        public string SaveAnImage(Bitmap img)
        {
            string imgPath = CreateImageFolder();
            img.Save(imgPath + "clientPrintScreen" + Guid.NewGuid() + ".jpg", ImageFormat.Jpeg);
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
