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

namespace Sys.Workflow.Engine.Impl.Variable
{
    /// 
    public class JodaDateTimeType : AbstractVariableType
    {

        public override string TypeName
        {
            get
            {
                return "jodadatetime";
            }
        }

        public override bool Cachable
        {
            get
            {
                return true;
            }
        }

        public override bool IsAbleToStore(object value)
        {
            if (value == null)
            {
                return true;
            }
            return value.GetType().IsAssignableFrom(typeof(DateTime));
        }

        public override object GetValue(IValueFields valueFields)
        {
            long? longValue = valueFields.LongValue;
            if (longValue != null)
            {
                return new DateTime(longValue.Value);
            }
            return null;
        }

        public override void SetValue(object value, IValueFields valueFields)
        {
            if (value != null)
            {
                valueFields.LongValue = ((DateTime)value).Ticks;
            }
            else
            {
                valueFields.LongValue = null;
            }
        }
    }

}