using Newtonsoft.Json.Linq;
using Sys.Bpm;
using System;
using System.Text;

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
namespace org.activiti.engine.impl.variable
{
    /// 
    public class LongJsonType : SerializableType
    {

        protected internal readonly int minLength;
        protected internal ObjectMapper objectMapper;

        public LongJsonType(int minLength, ObjectMapper objectMapper)
        {
            this.minLength = minLength;
            this.objectMapper = objectMapper;
        }

        public override string TypeName
        {
            get
            {
                return "longJson";
            }
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
                return jsonValue.ToString().Length >= minLength;
            }
            return false;
        }

        public override byte[] serialize(object value, IValueFields valueFields)
        {
            if (value == null)
            {
                return null;
            }
            JToken valueNode = (JToken)value;
            try
            {
                return valueNode.ToString().GetBytes(Encoding.UTF8);
            }
            catch (Exception e)
            {
                throw new ActivitiException("Error getting bytes from json variable", e);
            }
        }

        public override object deserialize(byte[] bytes, IValueFields valueFields)
        {
            JToken valueNode = null;
            try
            {
                valueNode = objectMapper.readTree(bytes);
            }
            catch (Exception e)
            {
                throw new ActivitiException("Error reading json variable", e);
            }
            return valueNode;
        }
    }

}