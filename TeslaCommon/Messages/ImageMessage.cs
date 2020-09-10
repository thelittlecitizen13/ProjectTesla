using System;
using System.Drawing;

namespace TeslaCommon
{
    [Serializable]
    public class ImageMessage : IMessage
    {
        public IMemberData Source { get; set; }
        public IMemberData Destination { get; set; }
        public DateTime MessageTime { get; set; }
        public Bitmap Image { get; set; }
        public ImageMessage(Bitmap img, UserData src, UserData dst)
        {
            Source = src;
            Destination = dst;
            Image = img;
            MessageTime = DateTime.Now;
        }
    }
}
