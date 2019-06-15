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

    /// <inheritdoc />
    public interface IProcessDefinitionEntity : IProcessDefinition, IEntity, IHasRevision
    {
        /// <inheritdoc />
        IList<IIdentityLinkEntity> IdentityLinks { get; }

        /// <inheritdoc />
        new string Id { get; set; }

        /// <inheritdoc />
        new string Key { get; set; }

        /// <inheritdoc />
        new string BusinessKey { get; set; }

        /// <inheritdoc />
        new string BusinessPath { get; set; }

        /// <inheritdoc />
        new string StartForm { get; set; }

        /// <inheritdoc />
        new string Name { get; set; }

        /// <inheritdoc />
        new string Description { get; set; }

        /// <inheritdoc />
        new string DeploymentId { get; set; }

        /// <inheritdoc />
        new int Version { get; set; }

        /// <inheritdoc />
        new string ResourceName { get; set; }

        /// <inheritdoc />
        new string TenantId { get; set; }

        /// <inheritdoc />
        int? HistoryLevel { get; set; }

        /// <inheritdoc />
        new string Category { get; set; }

        /// <inheritdoc />
        new string DiagramResourceName { get; set; }

        /// <inheritdoc />
        new bool HasStartFormKey { get; set; }

        /// <inheritdoc />
        new bool IsGraphicalNotationDefined { get; set; }

        /// <inheritdoc />
        int SuspensionState { get; set; }

        /// <inheritdoc />
        new string EngineVersion { get; set; }
    }
}