using System;
using System.Drawing;

namespace TeslaCommon
{
    [Serializable]
    public class ImageMessage : IMessage
    {
        public MemberData Source { get; set; }
        public MemberData Destination { get; set; }
        public DateTime MessageTime { get; set; }
        public Bitmap Image { get; set; }
        public ImageMessage(Bitmap img, MemberData src, MemberData dst)
        {
            Source = src;
            Destination = dst;
            Image = img;
            MessageTime = DateTime.Now;
        }
    }
}
