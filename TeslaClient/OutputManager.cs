using System.Diagnostics;
using System.Drawing;
using System.IO;


namespace TeslaClient
{
    public class OutputManager
    {
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
    }
}
