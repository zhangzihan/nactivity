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
namespace org.activiti.engine.impl.persistence.entity
{

    using org.activiti.engine.impl.db;
    using org.activiti.engine.repository;

    /// 
    /// 
    public interface IProcessDefinitionEntity : IProcessDefinition, IEntity, IHasRevision
    {

        IList<IIdentityLinkEntity> IdentityLinks { get; }

        new string Id { get; set; }

        new string Key { get; set; }

        new string BusinessKey { get; set; }

        new string BusinessPath { get; set; }

        new string StartForm { get; set; }

        new string Name { get; set; }

        new string Description { get; set; }

        new string DeploymentId { get; set; }

        new int Version { get; set; }

        new string ResourceName { get; set; }

        new string TenantId { get; set; }

        int? HistoryLevel { get; set; }


        new string Category { get; set; }

        new string DiagramResourceName { get; set; }

        new bool HasStartFormKey { get; set; }


        new bool IsGraphicalNotationDefined { get; set; }


        int SuspensionState { get; set; }


        new string EngineVersion { get; set; }


    }

}