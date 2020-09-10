using System.IO;
using System.Runtime.Serialization;

namespace TeslaCommon
{
    public interface ISerializer
    {
        IFormatter Formatter { get; set; }
        void Serialize(Stream stream, object obj);
        object Deserialize(Stream stream);
    }
}
