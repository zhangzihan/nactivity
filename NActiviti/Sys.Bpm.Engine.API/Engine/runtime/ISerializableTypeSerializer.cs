using System;
using System.Collections.Generic;
using System.Text;

namespace Sys.Runtime.Serialization
{
    public interface ISerializableTypeSerializer
    {
        object Deserialize(byte[] bytes);

        byte[] Serialize(object value);

        T Deserialize<T>(byte[] bytes);

        byte[] Serialize<T>(T value);
    }
}
