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
namespace org.activiti.engine.repository
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

        string Id { get; }

        string Name { get; set; }


        string Key { get; set; }


        string Category { get; set; }


        DateTime CreateTime { get; }

        DateTime LastUpdateTime { get; }

        int? Version { get; set; }


        string MetaInfo { get; set; }


        string DeploymentId { get; set; }


        string TenantId { set; get; }


        /// <summary>
        /// whether this model has editor source </summary>
        bool hasEditorSource();

        /// <summary>
        /// whether this model has editor source extra </summary>
        bool hasEditorSourceExtra();
    }

}