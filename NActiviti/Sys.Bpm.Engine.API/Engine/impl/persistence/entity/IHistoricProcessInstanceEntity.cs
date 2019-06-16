using System.Collections.Generic;

/*
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *       http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace Sys.Workflow.Engine.Impl.Persistence.Entity
{

    using Sys.Workflow.Engine.History;

    /// 
    public interface IHistoricProcessInstanceEntity : IHistoricScopeInstanceEntity, IHistoricProcessInstance
    {
        new string Id { get; set; }

        new string ProcessDefinitionId { get; set; }

        new string EndActivityId { get; set; }

        new string BusinessKey { get; set; }

        new string StartUserId { get; set; }

        new string StartUser { get; set; }

        new string StartActivityId { get; set; }

        new string SuperProcessInstanceId { get; set; }

        new string TenantId { get; set; }

        new string Name { get; set; }

        string LocalizedName { get; set; }

        new string Description { get; set; }

        string LocalizedDescription { get; set; }

        new string ProcessDefinitionKey { get; set; }


        new string ProcessDefinitionName { get; set; }


        new int? ProcessDefinitionVersion { get; set; }


        new string DeploymentId { get; set; }


        IList<IHistoricVariableInstanceEntity> QueryVariables { get; set; }


    }

}