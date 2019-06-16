using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Workflow
{
    public class ObjectMapper
    {
        public JToken ReadTree(string value)
        {
            return JsonConvert.DeserializeObject<JToken>(value);
        }

        public JToken ReadTree(byte[] infoBytes)
        {
            var str = Encoding.UTF8.GetString(infoBytes);

            return ReadTree(str);
        }

        public JToken CreateObjectNode()
        {
            return new JObject();
        }

        public byte[] WriteValueAsBytes(JToken data)
        {
            if (data == null)
            {
                return new byte[0];
            }

            var str = WriteValueAsString(data);

            return Encoding.UTF8.GetBytes(str);
        }

        public byte[] WriteValueAsBytes(IDictionary<string, object> data)
        {
            if (data == null)
            {
                return new byte[0];
            }

            var str = WriteValueAsString(data);

            return Encoding.UTF8.GetBytes(str);
        }

        public string WriteValueAsString(object variableValue)
        {
            if (variableValue == null)
            {
                return null;
            }

            return JsonConvert.SerializeObject(variableValue);
        }

        public JArray CreateArrayNode()
        {
            return new JArray();
        }
    }
}
