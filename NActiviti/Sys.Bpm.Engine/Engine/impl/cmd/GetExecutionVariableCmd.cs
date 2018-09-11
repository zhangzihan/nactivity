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
namespace org.activiti.engine.impl.cmd
{


    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;
    using org.activiti.engine.runtime;
    using System.Collections.Generic;

    /// 
    /// 
    [Serializable]
    public class GetExecutionVariableCmd : ICommand<object>
    {

        private const long serialVersionUID = 1L;
        protected internal string executionId;
        protected internal string variableName;
        protected internal bool isLocal;

        public GetExecutionVariableCmd(string executionId, string variableName, bool isLocal)
        {
            this.executionId = executionId;
            this.variableName = variableName;
            this.isLocal = isLocal;
        }

        public virtual object execute(ICommandContext commandContext)
        {
            if (string.ReferenceEquals(executionId, null))
            {
                throw new ActivitiIllegalArgumentException("executionId is null");
            }
            if (string.ReferenceEquals(variableName, null))
            {
                throw new ActivitiIllegalArgumentException("variableName is null");
            }

            IExecutionEntity execution = commandContext.ExecutionEntityManager.findById<IExecutionEntity>(new KeyValuePair<string, object>("id", executionId));

            if (execution == null)
            {
                throw new ActivitiObjectNotFoundException("execution " + executionId + " doesn't exist", typeof(IExecution));
            }

            object value;

            if (isLocal)
            {
                value = execution.getVariableLocal(variableName, false);
            }
            else
            {
                value = execution.getVariable(variableName, false);
            }

            return value;
        }
    }

}