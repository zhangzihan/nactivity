using Newtonsoft.Json.Linq;
using Sys.Workflow.Engine.Runtime;
using Sys.Net.Http;
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

namespace Sys.Workflow.Cloud.Services.Api.Model.Converters
{

    /// <summary>
    /// 
    /// </summary>
    public class ProcessInstanceConverter : IModelConverter<IProcessInstance, ProcessInstance>
    {
        private readonly ListConverter listConverter;


        /// <summary>
        /// 
        /// </summary>
        public ProcessInstanceConverter(ListConverter listConverter)
        {
            this.listConverter = listConverter;
        }


        /// <summary>
        /// 
        /// </summary>
        public virtual ProcessInstance From(IProcessInstance source)
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
                    StartUserId = source.StartUserId,
                    StartDate = source.StartTime,
                    BusinessKey = source.BusinessKey,
                    Status = CalculateStatus(source),
                    Starter = JToken.FromObject(source.Starter).ToObject<UserInfo>(),
                    ProcessDefinitionKey = source.ProcessDefinitionKey
                };
            }
            return processInstance;
        }


        /// <summary>
        /// 
        /// </summary>
        private string CalculateStatus(IProcessInstance source)
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


        /// <summary>
        /// 
        /// </summary>
        public virtual IEnumerable<ProcessInstance> From(IEnumerable<IProcessInstance> processInstances)
        {
            return listConverter.From(processInstances, this);
        }
    }

}