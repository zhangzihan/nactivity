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
    public class NullType : AbstractVariableType
    {

        private const long serialVersionUID = 1L;

        public override string TypeName
        {
            get
            {
                return "null";
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
            return null;
        }

        public override bool IsAbleToStore(object value)
        {
            return (value == null);
        }

        public override void SetValue(object value, IValueFields valueFields)
        {
        }
    }

}