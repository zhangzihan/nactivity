using Sys.Workflow.engine.history;
using Sys.Workflow.engine.runtime;
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

namespace Sys.Workflow.cloud.services.api.model.converter
{

    /// <summary>
    /// 
    /// </summary>
    public class HistoricInstanceConverter : IModelConverter<IHistoricProcessInstance, HistoricInstance>
    {
        private readonly ListConverter listConverter;


        /// <summary>
        /// 
        /// </summary>
        public HistoricInstanceConverter(ListConverter listConverter)
        {
            this.listConverter = listConverter;
        }


        /// <summary>
        /// 
        /// </summary>
        public virtual HistoricInstance From(IHistoricProcessInstance source)
        {
            HistoricInstance processInstance = null;
            if (source != null)
            {
                processInstance = new HistoricInstance()
                {
                    Id = source.Id,
                    Name = source.Name,
                    Description = source.Description,
                    ProcessDefinitionId = source.ProcessDefinitionId,
                    StartUserId = source.StartUserId,
                    StartDate = source.StartTime,
                    EndDate = source.EndTime,
                    BusinessKey = source.BusinessKey,
                    Status = CalculateStatus(source),
                    ProcessDefinitionKey = source.ProcessDefinitionKey,
                    DeleteReason = source.DeleteReason
                };
            }
            return processInstance;
        }


        /// <summary>
        /// 
        /// </summary>
        private string CalculateStatus(IHistoricProcessInstance source)
        {
            if (string.IsNullOrWhiteSpace(source.DeleteReason))
            {
                return Enum.GetName(typeof(HistoricInstance.HistoricInstanceStatus), HistoricInstance.HistoricInstanceStatus.COMPLETED);
            }
            else
            {
                return Enum.GetName(typeof(HistoricInstance.HistoricInstanceStatus), HistoricInstance.HistoricInstanceStatus.DELETED);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public virtual IEnumerable<HistoricInstance> From(IEnumerable<IHistoricProcessInstance> processInstances)
        {
            return listConverter.From(processInstances, this);
        }
    }

}