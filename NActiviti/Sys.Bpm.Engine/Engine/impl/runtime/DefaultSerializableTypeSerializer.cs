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
        // Ceras 初始化开销较大，且 JSON 路径用不到，改为延迟构造，仅在泛型序列化路径首次使用时初始化
        private readonly Lazy<CerasSerializer> serializer = new Lazy<CerasSerializer>(() => new CerasSerializer());

        // JSON 序列化配置共享静态实例（运行期不修改，可安全复用）；不再使用 Formatting.Indented 减少输出体积
        // TypeNameHandling.All 与存量序列化数据（含 $type）兼容，暂保留，后续可评估是否移除
        private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            TypeNameHandling = TypeNameHandling.All
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public object Deserialize(byte[] bytes)
        {
            string s = Encoding.UTF8.GetString(bytes);

            // 原实现 catch 后用完全相同的设置重试，必然再次失败，属无意义重试，已删除
            return JsonConvert.DeserializeObject(s, jsonSettings);
        }

        public T Deserialize<T>(byte[] bytes)
        {
            return serializer.Value.Deserialize<T>(bytes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] Serialize(object value)
        {
            string s = JsonConvert.SerializeObject(value, jsonSettings);
            return Encoding.UTF8.GetBytes(s);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] Serialize<T>(T value)
        {
            return serializer.Value.Serialize(value);
        }
    }
}
