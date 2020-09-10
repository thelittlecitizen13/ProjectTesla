using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace TeslaCommon
{
    public class BinarySerializer : ISerializer
    {
        public IFormatter Formatter { get; set; }
        public BinarySerializer()
        {
            Formatter = new BinaryFormatter();
        }
        public object Deserialize(Stream stream)
        {
            return Formatter.Deserialize(stream);
        }

        public void Serialize(Stream stream, object obj)
        {
            Formatter.Serialize(stream, obj);
        }
    }
}
