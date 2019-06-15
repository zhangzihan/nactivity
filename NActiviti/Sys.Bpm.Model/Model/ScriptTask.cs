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
    public class ScriptTask : TaskActivity
    {

        protected internal string scriptFormat;
        protected internal string script;
        protected internal string resultVariable;
        protected internal bool autoStoreVariables = false; // see
                                                            // https://activiti.atlassian.net/browse/ACT-1626

        public virtual string ScriptFormat
        {
            get
            {
                return scriptFormat;
            }
            set
            {
                this.scriptFormat = value;
            }
        }


        public virtual string Script
        {
            get
            {
                return script;
            }
            set
            {
                this.script = value;
            }
        }


        public virtual string ResultVariable
        {
            get
            {
                return resultVariable;
            }
            set
            {
                this.resultVariable = value;
            }
        }


        public virtual bool AutoStoreVariables
        {
            get
            {
                return autoStoreVariables;
            }
            set
            {
                this.autoStoreVariables = value;
            }
        }


        public override BaseElement Clone()
        {
            ScriptTask clone = new ScriptTask
            {
                Values = this
            };
            return clone;
        }

        public override BaseElement Values
        {
            set
            {
                base.Values = value;
                var val = value as ScriptTask;
                ScriptFormat = val.ScriptFormat;
                Script = val.Script;
                ResultVariable = val.ResultVariable;
                AutoStoreVariables = val.AutoStoreVariables;
            }
        }
    }

}