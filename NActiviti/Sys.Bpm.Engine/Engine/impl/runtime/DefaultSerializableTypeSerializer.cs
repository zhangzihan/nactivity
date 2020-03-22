using Ceras;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sys.Runtime.Serialization
{
    /// <summary>
    /// 默认变量序列化工具
    /// </summary>
    public class DefaultSerializableTypeSerializer : ISerializableTypeSerializer
    {
        private CerasSerializer serializer;

        public DefaultSerializableTypeSerializer()
        {
            serializer = new CerasSerializer();
        }

        public object Deserialize(byte[] bytes)
        {
            string s = Encoding.UTF8.GetString(bytes);

            try
            {
                return JsonConvert.DeserializeObject(s, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    TypeNameHandling = TypeNameHandling.All
                });
            }
            catch (JsonSerializationException e)
            {
                if (e.InnerException is FileNotFoundException)
                {
                    return JsonConvert.DeserializeObject(s, new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        TypeNameHandling = TypeNameHandling.All
                    });
                }
                throw e;
            }
        }

        public T Deserialize<T>(byte[] bytes)
        {
            return serializer.Deserialize<T>(bytes);
        }

        public byte[] Serialize(object value)
        {
            string s = JsonConvert.SerializeObject(value, Formatting.Indented,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    TypeNameHandling = TypeNameHandling.All
                });
            return Encoding.UTF8.GetBytes(s);

            //return serializer.Serialize(value);
        }

        public byte[] Serialize<T>(T value)
        {
            return serializer.Serialize(value);
        }
    }
}
