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
namespace Sys.Workflow.engine.repository
{

    /// <summary>
    /// Represents a model that is stored in the model repository. In addition, a model can be deployed to the Activiti Engine in a separate deployment step.
    /// 
    /// A model is a container for the meta data and sources of a process model that typically can be edited in a modeling environment.
    /// 
    /// 
    /// 
    /// </summary>
    public interface IModel
    {
        /// <summary>
        /// 
        /// </summary>
        string Id { get; }

        /// <summary>
        /// 
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// /
        /// </summary>
        string Key { get; set; }

        /// <summary>
        /// /
        /// </summary>
        string Category { get; set; }

        /// <summary>
        /// 
        /// </summary>
        DateTime CreateTime { get; }

        /// <summary>
        /// 
        /// </summary>
        DateTime LastUpdateTime { get; }

        /// <summary>
        /// 
        /// </summary>
        int? Version { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string MetaInfo { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string DeploymentId { get; set; }

        /// <summary>
        /// /
        /// </summary>
        string TenantId { set; get; }


        /// <summary>
        /// whether this model has editor source </summary>
        bool HasEditorSource();

        /// <summary>
        /// whether this model has editor source extra </summary>
        bool HasEditorSourceExtra();
    }
}