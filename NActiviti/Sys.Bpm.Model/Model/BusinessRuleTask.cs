using System.Collections.Generic;

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

    public class BusinessRuleTask : Task
    {

        protected internal string resultVariableName;
        protected internal bool exclude;
        protected internal IList<string> ruleNames = new List<string>();
        protected internal IList<string> inputVariables = new List<string>();
        protected internal string className;

        public virtual bool Exclude
        {
            get
            {
                return exclude;
            }
            set
            {
                this.exclude = value;
            }
        }


        public virtual string ResultVariableName
        {
            get
            {
                return resultVariableName;
            }
            set
            {
                this.resultVariableName = value;
            }
        }


        public virtual IList<string> RuleNames
        {
            get
            {
                return ruleNames;
            }
            set
            {
                this.ruleNames = value;
            }
        }


        public virtual IList<string> InputVariables
        {
            get
            {
                return inputVariables;
            }
            set
            {
                this.inputVariables = value;
            }
        }


        public virtual string ClassName
        {
            get
            {
                return className;
            }
            set
            {
                this.className = value;
            }
        }


        public override BaseElement clone()
        {
            BusinessRuleTask clone = new BusinessRuleTask();
            clone.Values = this;
            return clone;
        }

        public override BaseElement Values
        {
            set
            {
                base.Values = value;
                var val = value as BusinessRuleTask;

                ResultVariableName = val.ResultVariableName;
                Exclude = val.Exclude;
                ClassName = val.ClassName;
                ruleNames = new List<string>(val.RuleNames);
                inputVariables = new List<string>(val.InputVariables);
            }
        }
    }

}