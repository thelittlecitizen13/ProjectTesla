using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using TeslaCommon;

namespace TeslaClient
{
    public class PicturesCommandHandler
    {
        private TeslaClient _teslaClient;
        public PicturesCommandHandler(TeslaClient teslaClient)
        {
            _teslaClient = teslaClient;
        }
        public IMessage GeneratePictureMessage(string[] args, IMemberData source, IMemberData destination)
        {
            Bitmap img = null;
            if (args.Length == 1) // == /picture
                img = takeScreenShot();
            else
                img = loadImage(args[1]);
            try
            {
                if (img != null)
                    return new ImageMessage(img, (UserData)source, (UserData)destination);
            }
            catch
            {
                return null;
            }
            return null;

        }
        private Bitmap takeScreenShot()
        {
            var bitmap = new Bitmap(1920, 1080);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(0, 0, 0, 0,
                bitmap.Size, CopyPixelOperation.SourceCopy);
            }
            string imageFolderPath = _teslaClient.clientData.Outputter.CreateImageFolder();
            bitmap.Save(imageFolderPath + "clientPrintScreen" + Guid.NewGuid() + ".jpg", ImageFormat.Jpeg);
            return bitmap;
        }
        private Bitmap loadImage(string path)
        {
            try
            {
                return new Bitmap(path);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }
    }
}
