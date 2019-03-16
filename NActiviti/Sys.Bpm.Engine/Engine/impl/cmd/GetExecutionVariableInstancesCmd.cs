using System;
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
namespace org.activiti.engine.impl.cmd
{


    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;
    using org.activiti.engine.runtime;

    [Serializable]
    public class GetExecutionVariableInstancesCmd : ICommand<IDictionary<string, IVariableInstance>>
    {
        private const long serialVersionUID = 1L;
        protected internal string executionId;
        protected internal ICollection<string> variableNames;
        protected internal bool isLocal;

        public GetExecutionVariableInstancesCmd(string executionId, ICollection<string> variableNames, bool isLocal)
        {
            this.executionId = executionId;
            this.variableNames = variableNames;
            this.isLocal = isLocal;
        }

        public virtual IDictionary<string, IVariableInstance> execute(ICommandContext commandContext)
        {

            // Verify existance of execution
            if (ReferenceEquals(executionId, null))
            {
                throw new ActivitiIllegalArgumentException("executionId is null");
            }

            IExecutionEntity execution = commandContext.ExecutionEntityManager.findById<IExecutionEntity>(executionId);

            if (execution == null)
            {
                throw new ActivitiObjectNotFoundException("execution " + executionId + " doesn't exist", typeof(IExecution));
            }

            IDictionary<string, IVariableInstance> variables = null;
            if (variableNames == null || variableNames.Count == 0)
            {
                // Fetch all
                if (isLocal)
                {
                    variables = execution.VariableInstancesLocal;
                }
                else
                {
                    variables = execution.VariableInstances;
                }

            }
            else
            {
                // Fetch specific collection of variables
                if (isLocal)
                {
                    variables = execution.getVariableInstancesLocal(variableNames, false);
                }
                else
                {
                    variables = execution.getVariableInstances(variableNames, false);
                }
            }

            return variables;
        }
    }

}