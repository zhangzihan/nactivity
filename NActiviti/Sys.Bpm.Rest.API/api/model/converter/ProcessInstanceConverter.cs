using org.activiti.engine.runtime;
using System;
using System.Collections.Generic;

/*
 * Licensed under the Apache License, Version 2.0 (the "License");
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
 *
 */

namespace org.activiti.cloud.services.api.model.converter
{
    public class ProcessInstanceConverter : IModelConverter<IProcessInstance, ProcessInstance>
    {
        private readonly ListConverter listConverter;

        public ProcessInstanceConverter(ListConverter listConverter)
        {
            this.listConverter = listConverter;
        }

        public virtual ProcessInstance from(IProcessInstance source)
        {
            ProcessInstance processInstance = null;
            if (source != null)
            {
                processInstance = new ProcessInstance()
                {
                    Id = source.Id,
                    Name = source.Name,
                    Description = source.Description,
                    ProcessDefinitionId = source.ProcessDefinitionId,
                    Initiator = source.StartUserId,
                    StartDate = source.StartTime,
                    BusinessKey = source.BusinessKey,
                    Status = calculateStatus(source),
                    ProcessDefinitionKey = source.ProcessDefinitionKey
                };
            }
            return processInstance;
        }

        private string calculateStatus(IProcessInstance source)
        {
            if (source.Suspended)
            {
                return Enum.GetName(typeof(ProcessInstance.ProcessInstanceStatus), ProcessInstance.ProcessInstanceStatus.SUSPENDED);
            }
            else if (source.Ended)
            {
                return Enum.GetName(typeof(ProcessInstance.ProcessInstanceStatus), ProcessInstance.ProcessInstanceStatus.COMPLETED);
            }
            return Enum.GetName(typeof(ProcessInstance.ProcessInstanceStatus), ProcessInstance.ProcessInstanceStatus.RUNNING);
        }

        public virtual IList<org.activiti.cloud.services.api.model.ProcessInstance> from(IList<IProcessInstance> processInstances)
        {
            return listConverter.from(processInstances, this);
        }
    }

}