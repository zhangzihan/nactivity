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
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Sys;
using Sys.Bpm;
using System;
using System.Text;

namespace org.activiti.engine.impl.variable
{
    /// 
    public class JsonType : AbstractVariableType
    {
        private static readonly ILogger<JsonType> log = ProcessEngineServiceProvider.LoggerService<JsonType>();

        protected internal readonly int maxLength;
        protected internal ObjectMapper objectMapper;

        public JsonType(int maxLength, ObjectMapper objectMapper)
        {
            this.maxLength = maxLength;
            this.objectMapper = objectMapper;
        }

        public override string TypeName
        {
            get
            {
                return "json";
            }
        }

        public override bool Cachable
        {
            get
            {
                return true;
            }
        }

        public override object getValue(IValueFields valueFields)
        {
            JToken jsonValue = null;
            if (!ReferenceEquals(valueFields.TextValue, null) && valueFields.TextValue.Length > 0)
            {
                try
                {
                    jsonValue = objectMapper.readTree(valueFields.TextValue);
                }
                catch (Exception e)
                {
                    log.LogError(e, $"Error reading json variable {valueFields.Name}");
                }
            }
            return jsonValue;
        }

        public override void setValue(object value, IValueFields valueFields)
        {
            valueFields.TextValue = value != null ? value.ToString() : null;
        }

        public override bool isAbleToStore(object value)
        {
            if (value == null)
            {
                return true;
            }
            if (value.GetType().IsAssignableFrom(typeof(JToken)))
            {
                JToken jsonValue = (JToken)value;
                return jsonValue.ToString().Length <= maxLength;
            }
            return false;
        }
    }

}