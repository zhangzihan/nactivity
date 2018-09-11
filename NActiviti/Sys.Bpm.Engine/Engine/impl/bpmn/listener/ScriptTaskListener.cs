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

namespace org.activiti.engine.impl.bpmn.listener
{
    using org.activiti.engine.@delegate;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.scripting;

    /// 
    /// 
    [Serializable]
    public class ScriptTaskListener : ITaskListener
    {

        private const long serialVersionUID = -8915149072830499057L;

        protected internal IExpression script;

        protected internal IExpression language;

        protected internal IExpression resultVariable;

        protected internal bool autoStoreVariables;

        public virtual void notify(IDelegateTask delegateTask)
        {
            throw new System.NotImplementedException();
            //validateParameters();

            //ScriptingEngines scriptingEngines = Context.ProcessEngineConfiguration.ScriptingEngines;
            //object result = scriptingEngines.evaluate(script.ExpressionText, language.ExpressionText, delegateTask, autoStoreVariables);

            //if (resultVariable != null)
            //{
            //  delegateTask.setVariable(resultVariable.ExpressionText, result);
            //}
        }

        protected internal virtual void validateParameters()
        {
            if (script == null)
            {
                throw new System.ArgumentException("The field 'script' should be set on the TaskListener");
            }

            if (language == null)
            {
                throw new System.ArgumentException("The field 'language' should be set on the TaskListener");
            }
        }

        public virtual IExpression Script
        {
            set
            {
                this.script = value;
            }
        }

        public virtual IExpression Language
        {
            set
            {
                this.language = value;
            }
        }

        public virtual IExpression ResultVariable
        {
            set
            {
                this.resultVariable = value;
            }
        }

        public virtual bool AutoStoreVariables
        {
            set
            {
                this.autoStoreVariables = value;
            }
        }

    }

}