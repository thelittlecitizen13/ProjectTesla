using System;
using System.Drawing;

namespace TeslaCommon
{
    public class ImagePacket : IPacket
    {
        public ClientData Source { get; set; }
        public ClientData Destination { get; set; }
        public DateTime MessageTime { get; set; }
        public Bitmap Image { get; set; }
        public ImagePacket(Bitmap img, ClientData src, ClientData dst)
        {
            Source = src;
            Destination = dst;
            Image = img;
            MessageTime = DateTime.Now;
        }
    }
}
