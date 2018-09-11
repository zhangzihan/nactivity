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
    public class IOParameter : BaseElement
    {

        protected internal string source;
        protected internal string sourceExpression;
        protected internal string target;
        protected internal string targetExpression;

        public virtual string Source
        {
            get
            {
                return source;
            }
            set
            {
                this.source = value;
            }
        }


        public virtual string Target
        {
            get
            {
                return target;
            }
            set
            {
                this.target = value;
            }
        }


        public virtual string SourceExpression
        {
            get
            {
                return sourceExpression;
            }
            set
            {
                this.sourceExpression = value;
            }
        }


        public virtual string TargetExpression
        {
            get
            {
                return targetExpression;
            }
            set
            {
                this.targetExpression = value;
            }
        }


        public override BaseElement clone()
        {
            IOParameter clone = new IOParameter();
            clone.Values = this;
            return clone;
        }

        public override BaseElement Values
        {
            set
            {
                base.Values = value;
                var val = value as IOParameter;

                Source = val.Source;
                SourceExpression = val.SourceExpression;
                Target = val.Target;
                TargetExpression = val.TargetExpression;
            }
        }
    }

}