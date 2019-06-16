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
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sys.Workflow.engine.impl.persistence.entity
{
    /// <summary>
    /// Contains a predefined set of states for process definitions and process instances
    /// 
    /// 
    /// </summary>
    [Newtonsoft.Json.JsonConverter(typeof(SuspensionStateJsonConverter))]
    public interface ISuspensionState
    {

        int StateCode { get; set; }

        string Name { get; set; }

        // default implementation /////////////////////////////////////////////////// 

        // helper class ///////////////////////////////////////// 

    }

    public class SuspensionStateJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string json = reader.Value?.ToString();
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            JToken ss = JToken.Parse(json);

            if (int.TryParse(ss["stateCode"]?.ToString(), out int code))
            {
                return new SuspensionStateImpl(code, ss["name"]?.ToString());
            }

            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                return;
            }

            ISuspensionState ss = value as ISuspensionState;

            writer.WriteValue("{\"stateCode\":" + ss.StateCode + ",\"name\":\"" + ss.Name + "\"}");
        }
    }
}