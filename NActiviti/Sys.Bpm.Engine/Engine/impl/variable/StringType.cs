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
    public class StringType : AbstractVariableType
    {

        private readonly int maxLength;

        public StringType(int maxLength)
        {
            this.maxLength = maxLength;
        }

        public override string TypeName
        {
            get
            {
                return "string";
            }
        }

        public override bool Cachable
        {
            get
            {
                return true;
            }
        }

        public override object GetValue(IValueFields valueFields)
        {
            return valueFields.TextValue;
        }

        public override void SetValue(object value, IValueFields valueFields)
        {
            valueFields.TextValue = (string)value;
        }

        public override bool IsAbleToStore(object value)
        {
            if (value == null)
            {
                return true;
            }
            if (value.GetType().IsAssignableFrom(typeof(string)))
            {
                string stringValue = (string)value;
                return stringValue.Length <= maxLength;
            }
            return false;
        }
    }

}