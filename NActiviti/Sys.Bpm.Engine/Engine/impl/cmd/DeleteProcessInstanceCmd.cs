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
    [Serializable]
    public class DeleteProcessInstanceCmd : ICommand<object>
    {

        private const long serialVersionUID = 1L;
        protected internal string processInstanceId;
        protected internal string deleteReason;

        public DeleteProcessInstanceCmd(string processInstanceId, string deleteReason)
        {
            this.processInstanceId = processInstanceId;
            this.deleteReason = deleteReason;
        }

        public virtual object execute(ICommandContext commandContext)
        {
            if (ReferenceEquals(processInstanceId, null))
            {
                throw new ActivitiIllegalArgumentException("processInstanceId is null");
            }

            IExecutionEntity processInstanceEntity = commandContext.ExecutionEntityManager.findById<IExecutionEntity>(new KeyValuePair<string, object>("id", processInstanceId));

            if (processInstanceEntity == null)
            {
                throw new ActivitiObjectNotFoundException("No process instance found for id '" + processInstanceId + "'", typeof(IProcessInstance));
            }

            commandContext.ExecutionEntityManager.deleteProcessInstance(processInstanceEntity.ProcessInstanceId, deleteReason, false);

            return null;
        }

    }

}