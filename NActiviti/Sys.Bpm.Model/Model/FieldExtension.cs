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
namespace org.activiti.bpmn.model
{
    public class FieldExtension : BaseElement
    {

        protected internal string fieldName;
        protected internal string stringValue;
        protected internal string expression;

        public FieldExtension()
        {

        }

        public virtual string FieldName
        {
            get
            {
                return fieldName;
            }
            set
            {
                this.fieldName = value;
            }
        }


        public virtual string StringValue
        {
            get
            {
                return stringValue;
            }
            set
            {
                this.stringValue = value;
            }
        }


        public virtual string Expression
        {
            get
            {
                return expression;
            }
            set
            {
                this.expression = value;
            }
        }


        public override BaseElement clone()
        {
            FieldExtension clone = new FieldExtension();
            clone.Values = this;
            return clone;
        }

        public override BaseElement Values
        {
            set
            {
                var val = value as FieldExtension;
                FieldName = val.FieldName;
                StringValue = val.StringValue;
                Expression = val.Expression;
            }
        }
    }

}