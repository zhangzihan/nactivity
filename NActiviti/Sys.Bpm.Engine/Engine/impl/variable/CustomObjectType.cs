using System;

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
    /// <summary>
    /// Custom object type
    /// 
    /// 
    /// </summary>
    public class CustomObjectType : AbstractVariableType
    {

        protected internal string typeName;
        protected internal Type theClass;

        public CustomObjectType(string typeName, Type theClass)
        {
            this.theClass = theClass;
            this.typeName = typeName;
        }

        public override string TypeName
        {
            get
            {
                return this.typeName;
            }
        }

        public override object getValue(IValueFields valueFields)
        {
            return valueFields.CachedValue;
        }

        public override bool isAbleToStore(object value)
        {
            if (value == null)
            {
                return true;
            }
            return this.theClass.IsAssignableFrom(value.GetType());
        }

        public override bool Cachable
        {
            get
            {
                return true;
            }
        }

        public override void setValue(object value, IValueFields valueFields)
        {
            valueFields.CachedValue = value;
        }
    }

}