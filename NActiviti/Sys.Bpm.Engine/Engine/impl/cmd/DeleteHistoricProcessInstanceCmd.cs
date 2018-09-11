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


    using org.activiti.engine.history;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.util;
    using System.Collections.Generic;

    /// 
    [Serializable]
    public class DeleteHistoricProcessInstanceCmd : ICommand<object>
    {

        private const long serialVersionUID = 1L;
        protected internal string processInstanceId;

        public DeleteHistoricProcessInstanceCmd(string processInstanceId)
        {
            this.processInstanceId = processInstanceId;
        }

        public virtual object execute(ICommandContext commandContext)
        {
            if (string.ReferenceEquals(processInstanceId, null))
            {
                throw new ActivitiIllegalArgumentException("processInstanceId is null");
            }
            // Check if process instance is still running
            IHistoricProcessInstance instance = commandContext.HistoricProcessInstanceEntityManager.findById<IHistoricProcessInstance>(new KeyValuePair<string, object>("id", processInstanceId));

            if (instance == null)
            {
                throw new ActivitiObjectNotFoundException("No historic process instance found with id: " + processInstanceId, typeof(IHistoricProcessInstance));
            }
            if (instance.EndTime == null)
            {
                throw new ActivitiException("Process instance is still running, cannot delete historic process instance: " + processInstanceId);
            }

            commandContext.HistoricProcessInstanceEntityManager.delete(new KeyValuePair<string, object>("id", processInstanceId));

            return null;
        }

    }

}