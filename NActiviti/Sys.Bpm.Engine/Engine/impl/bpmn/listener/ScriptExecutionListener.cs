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

namespace Sys.Workflow.Engine.Impl.Bpmn.Listeners
{
    using Sys.Workflow.Engine.Delegate;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;

    [Serializable]
    public class ScriptExecutionListener : IExecutionListener
    {

        private const long serialVersionUID = 1L;

        protected internal IExpression script;

        protected internal IExpression language;

        protected internal IExpression resultVariable;

        public virtual void Notify(IExecutionEntity execution)
        {
            //validateParameters();
            throw new NotImplementedException();

            //ScriptingEngines scriptingEngines = Context.ProcessEngineConfiguration.ScriptingEngines;
            //object result = scriptingEngines.evaluate(script.ExpressionText, language.ExpressionText, execution);

            //if (resultVariable is object)
            //{
            //  execution.setVariable(resultVariable.ExpressionText, result);
            //}
        }

        protected internal virtual void ValidateParameters()
        {
            if (script is null)
            {
                throw new ArgumentException("The field 'script' should be set on the ExecutionListener");
            }

            if (language is null)
            {
                throw new ArgumentException("The field 'language' should be set on the ExecutionListener");
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
    }

}