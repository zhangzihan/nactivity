using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Bpm
{
    public class ObjectMapper
    {
        public JToken readTree(string value)
        {
            return JsonConvert.DeserializeObject<JToken>(value);
        }

        public JToken readTree(byte[] infoBytes)
        {
            var str = Encoding.UTF8.GetString(infoBytes);

            return readTree(str);
        }

        public JToken createObjectNode()
        {
            return new JObject();
        }

        public byte[] writeValueAsBytes(JToken data)
        {
            if (data == null)
            {
                return new byte[0];
            }

            var str = writeValueAsString(data);

            return Encoding.UTF8.GetBytes(str);
        }

        public byte[] writeValueAsBytes(IDictionary<string, object> data)
        {
            if (data == null)
            {
                return new byte[0];
            }

            var str = writeValueAsString(data);

            return Encoding.UTF8.GetBytes(str);
        }

        public string writeValueAsString(object variableValue)
        {
            if (variableValue == null)
            {
                return null;
            }

            return JsonConvert.SerializeObject(variableValue);
        }

        public JArray createArrayNode()
        {
            return new JArray();
        }
    }
}
