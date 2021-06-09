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

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class JsonListConverter<T>
    {

        internal JsonObjectConverter<T> jsonObjectConverter;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonObjectConverter"></param>
        public JsonListConverter(JsonObjectConverter<T> jsonObjectConverter)
        {
            this.jsonObjectConverter = jsonObjectConverter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <param name="writer"></param>
        public virtual void ToJson(IList<T> list, StreamWriter writer)
        {
            writer.Write(ToJson(list, 1));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public virtual string ToJson(IList<T> list)
        {
            return ToJson(list, 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <param name="indentFactor"></param>
        /// <returns></returns>
        public virtual string ToJson(IList<T> list, int indentFactor = 0)
        {
            if (list is null)
            {
                return "";
            }

            return JsonConvert.SerializeObject(list, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public virtual IList<T> ToObject(StreamReader reader)
        {
            string str = reader.ReadToEnd();

            return JsonConvert.DeserializeObject<List<T>>(str);
        }

        /// <summary>
        /// 
        /// </summary>
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