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
namespace org.activiti.engine.impl.json
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System.IO;

    /// 
    public class JsonProcessEngineInfoConverter : JsonObjectConverter<IProcessEngineInfo>
    {

        public override JToken toJsonObject(IProcessEngineInfo processEngineInfo)
        {
            ProcessEngineInfoImpl processEngineInfoImpl = (ProcessEngineInfoImpl)processEngineInfo;
            JToken jsonObject = JToken.FromObject(new
            {
                name = processEngineInfoImpl.Name,
                resourceUrl = processEngineInfoImpl.ResourceUrl,
                exception = processEngineInfoImpl.Exception
            });

            return jsonObject;
        }

        public override IProcessEngineInfo toObject(StreamReader reader)
        {
            var str = reader.ReadToEnd();

            return JsonConvert.DeserializeObject<ProcessEngineInfoImpl>(str);
        }
    }

}