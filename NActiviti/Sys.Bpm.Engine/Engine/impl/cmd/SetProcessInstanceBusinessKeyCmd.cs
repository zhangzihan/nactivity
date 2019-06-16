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

namespace Sys.Workflow.Engine.Impl.Cmd
{

    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Runtime;
    using System.Collections.Generic;

    /// <summary>
    /// <seealso cref="Command"/> that changes the business key of an existing process instance.
    /// 
    /// 
    /// </summary>
    [Serializable]
    public class SetProcessInstanceBusinessKeyCmd : ICommand<object>
    {

        private const long serialVersionUID = 1L;

        private readonly string processInstanceId;
        private readonly string businessKey;

        public SetProcessInstanceBusinessKeyCmd(string processInstanceId, string businessKey)
        {
            if (processInstanceId is null || processInstanceId.Length < 1)
            {
                throw new ActivitiIllegalArgumentException("The process instance id is mandatory, but '" + processInstanceId + "' has been provided.");
            }
            if (businessKey is null)
            {
                throw new ActivitiIllegalArgumentException("The business key is mandatory, but 'null' has been provided.");
            }

            this.processInstanceId = processInstanceId;
            this.businessKey = businessKey;
        }

        public virtual object Execute(ICommandContext commandContext)
        {
            IExecutionEntityManager executionManager = commandContext.ExecutionEntityManager;
            IExecutionEntity processInstance = executionManager.FindById<IExecutionEntity>(new KeyValuePair<string, object>("id", processInstanceId));
            if (processInstance == null)
            {
                throw new ActivitiObjectNotFoundException("No process instance found for id = '" + processInstanceId + "'.", typeof(IProcessInstance));
            }
            else if (!processInstance.ProcessInstanceType)
            {
                throw new ActivitiIllegalArgumentException("A process instance id is required, but the provided id " + "'" + processInstanceId + "' " + "points to a child execution of process instance " + "'" + processInstance.ProcessInstanceId + "'. " + "Please invoke the " + this.GetType().Name + " with a root execution id.");
            }

            executionManager.UpdateProcessInstanceBusinessKey(processInstance, businessKey);

            return null;
        }
    }

}