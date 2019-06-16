using System.Collections.Generic;

/* Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace Sys.Workflow.Engine.Impl.Json
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System.IO;

    /// 
    public class JsonListConverter<T>
    {

        internal JsonObjectConverter<T> jsonObjectConverter;

        public JsonListConverter(JsonObjectConverter<T> jsonObjectConverter)
        {
            this.jsonObjectConverter = jsonObjectConverter;
        }

        public virtual void ToJson(IList<T> list, StreamWriter writer)
        {
            writer.Write(ToJson(list, 1));
        }

        public virtual string ToJson(IList<T> list)
        {
            return ToJsonArray(list).ToString();
        }

        public virtual string ToJson(IList<T> list, int indentFactor = 0)
        {
            if (list == null)
            {
                return "";
            }

            return JsonConvert.SerializeObject(list, indentFactor > 0 ? Formatting.Indented : Formatting.None);//.ToString(indentFactor);
        }

        private JArray ToJsonArray(IList<T> objects)
        {
            JArray jsonArray = new JArray();
            foreach (T @object in objects)
            {
                jsonArray.Add(jsonObjectConverter.ToJsonObject(@object));
            }
            return jsonArray;
        }

        public virtual IList<T> ToObject(StreamReader reader)
        {
            string str = reader.ReadToEnd();

            return JsonConvert.DeserializeObject<List<T>>(str);
        }

        public virtual JsonObjectConverter<T> JsonObjectConverter
        {
            get
            {
                return jsonObjectConverter;
            }
            set
            {
                this.jsonObjectConverter = value;
            }
        }

    }

}