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
namespace org.activiti.engine.impl.bpmn.behavior
{
    using org.activiti.engine.impl.persistence.entity;

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

        private const long serialVersionUID = 1L;

        protected internal string scriptTaskId;
        protected internal string script;
        protected internal string language;
        protected internal string resultVariable;
        protected internal bool storeScriptVariables = false; // see https://activiti.atlassian.net/browse/ACT-1626

        public ScriptTaskActivityBehavior(string script, string language, string resultVariable)
        {
            this.script = script;
            this.language = language;
            this.resultVariable = resultVariable;
        }

        public ScriptTaskActivityBehavior(string scriptTaskId, string script, string language, string resultVariable, bool storeScriptVariables) : this(script, language, resultVariable)
        {
            this.scriptTaskId = scriptTaskId;
            this.storeScriptVariables = storeScriptVariables;
        }

        public override void execute(IExecutionEntity execution)
        {
            throw new System.NotImplementedException();
            //ScriptingEngines scriptingEngines = Context.ProcessEngineConfiguration.ScriptingEngines;

            //if (Context.ProcessEngineConfiguration.EnableProcessDefinitionInfoCache)
            //{
            //    ObjectNode taskElementProperties = Context.getBpmnOverrideElementProperties(scriptTaskId, execution.ProcessDefinitionId);
            //    if (taskElementProperties != null && taskElementProperties.has(org.activiti.engine.DynamicBpmnConstants_Fields.SCRIPT_TASK_SCRIPT))
            //    {
            //        string overrideScript = taskElementProperties[org.activiti.engine.DynamicBpmnConstants_Fields.SCRIPT_TASK_SCRIPT].ToString();
            //        if (!string.IsNullOrWhiteSpace(overrideScript) && !overrideScript.Equals(script))
            //        {
            //            script = overrideScript;
            //        }
            //    }
            //}

            //bool noErrors = true;
            //try
            //{
            //    object result = scriptingEngines.evaluate(script, language, execution, storeScriptVariables);

            //    if (!string.ReferenceEquals(resultVariable, null))
            //    {
            //        execution.setVariable(resultVariable, result);
            //    }

            //}
            //catch (ActivitiException e)
            //{

            //    //LOGGER.warn("Exception while executing " + execution.CurrentFlowElement.Id + " : " + e.Message);

            //    noErrors = false;
            //    Exception rootCause = e;//ExceptionUtils.getRootCause(e);
            //    if (rootCause is BpmnError)
            //    {
            //        ErrorPropagation.propagateError((BpmnError)rootCause, execution);
            //    }
            //    else
            //    {
            //        throw e;
            //    }
            //}
            //if (noErrors)
            //{
            //    leave(execution);
            //}
        }

    }

}