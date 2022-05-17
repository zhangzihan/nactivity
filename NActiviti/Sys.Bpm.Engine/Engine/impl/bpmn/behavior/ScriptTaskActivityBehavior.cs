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
namespace Sys.Workflow.Engine.Impl.Bpmn.Behavior
{
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;
    using Sys.Workflow.Engine.Delegate;
    using Sys.Workflow.Engine.Impl.Bpmn.Helper;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Impl.Scripting;
    using Sys.Workflow;

    /// <summary>
    /// activity implementation of the BPMN 2.0 script task.
    /// 
    /// 
    /// 
    /// 
    /// </summary>
    [Serializable]
    public class ScriptTaskActivityBehavior : TaskActivityBehavior
    {
        private static readonly ILogger logger = ProcessEngineServiceProvider.LoggerService<ScriptTaskActivityBehavior>();

        private const long serialVersionUID = 1L;
        /// <summary>
        /// 
        /// </summary>
        protected internal string scriptTaskId;
        /// <summary>
        /// 
        /// </summary>
        protected internal string script;
        /// <summary>
        /// 
        /// </summary>
        protected internal string language;
        /// <summary>
        /// 
        /// </summary>
        protected internal string resultVariable;
        /// <summary>
        /// 
        /// </summary>
        protected internal bool storeScriptVariables = false; // see https://activiti.atlassian.net/browse/ACT-1626

        public ScriptTaskActivityBehavior(string script, string language, string resultVariable)
        {
            this.script = script;
            this.language = language;
            this.resultVariable = resultVariable;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scriptTaskId"></param>
        /// <param name="script"></param>
        /// <param name="language"></param>
        /// <param name="resultVariable"></param>
        /// <param name="storeScriptVariables"></param>
        public ScriptTaskActivityBehavior(string scriptTaskId, string script, string language, string resultVariable, bool storeScriptVariables) : this(script, language, resultVariable)
        {
            this.scriptTaskId = scriptTaskId;
            this.storeScriptVariables = storeScriptVariables;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        public override void Execute(IExecutionEntity execution)
        {
            IScriptingEngines scriptingEngines = Context.ProcessEngineConfiguration.ScriptingEnginesProvider.Create(language);

            if (Context.ProcessEngineConfiguration.EnableProcessDefinitionInfoCache)
            {
                JToken taskElementProperties = Context.GetBpmnOverrideElementProperties(scriptTaskId, execution.ProcessDefinitionId);
                if (taskElementProperties is not null && taskElementProperties[DynamicBpmnConstants.SCRIPT_TASK_SCRIPT] is not null)
                {
                    string overrideScript = taskElementProperties[DynamicBpmnConstants.SCRIPT_TASK_SCRIPT].ToString();
                    if (!string.IsNullOrWhiteSpace(overrideScript) && !overrideScript.Equals(script))
                    {
                        script = overrideScript;
                    }
                }
            }

            bool noErrors = true;
            try
            {
                object result = scriptingEngines.Evaluate(script, execution);

                if (resultVariable is not null)
                {
                    execution.SetVariable(resultVariable, result);
                }
            }
            catch (ActivitiException e)
            {
                logger.LogWarning("Exception while executing " + execution.CurrentFlowElement.Id + " : " + e.Message);

                noErrors = false;
                if (e is BpmnError error)
                {
                    ErrorPropagation.PropagateError(error, execution);
                }
                else
                {
                    throw e;
                }
            }
            if (noErrors)
            {
                Leave(execution);
            }
        }
    }
}