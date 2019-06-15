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

namespace org.activiti.engine.impl.persistence.entity
{

    using org.activiti.engine.repository;

    /// 
    /// 
    public interface IDeploymentEntity : IDeployment, IEntity
    {
        new string Id { get; set; }

        void AddResource(IResourceEntity resource);

        IDictionary<string, IResourceEntity> GetResources();

        void SetResources(IDictionary<string, IResourceEntity> value);

        void AddDeployedArtifact(object deployedArtifact);

        void Unrunable();

        void Runable();

        void DeployExecutionBehavior();

        IList<T> GetDeployedArtifacts<T>();

        new string Name { get; set; }

        new string Category { get; set; }

        new string Key { get; set; }

        new string TenantId { get; set; }


        new DateTime DeploymentTime { get; set; }

        bool New { get; set; }


        string EngineVersion { get; set; }

        new string BusinessKey { get; set; }

        new string BusinessPath { get; set; }

        new string StartForm { get; set; }

    }
}