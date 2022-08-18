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
namespace Sys.Workflow.Engine.Impl.Variable
{
    /// 
    public class ShortType : AbstractVariableType
    {

        private const long serialVersionUID = 1L;

        public override string TypeName
        {
            get
            {
                return "short";
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
            if (valueFields.LongValue is not null)
            {
                return (short?)valueFields.LongValue;
            }
            return null;
        }

        public override void SetValue(object value, IValueFields valueFields)
        {
            if (value is not null)
            {
                valueFields.LongValue = ((short?)value).Value;
                valueFields.TextValue = value.ToString();
            }
            else
            {
                valueFields.LongValue = null;
                valueFields.TextValue = null;
            }
        }

        public override bool IsAbleToStore(object value)
        {
            if (value is null)
            {
                return true;
            }
            return value.GetType().IsAssignableFrom(typeof(short)) || value.GetType().IsAssignableFrom(typeof(short));
        }
    }

}