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
    /// An object structure representing an executable process composed of activities and transitions.
    /// 
    /// Business processes are often created with graphical editors that store the process definition in certain file format. These files can be added to a <seealso cref="IDeployment"/> artifact, such as for example
    /// a Business Archive (.bar) file.
    /// 
    /// At deploy time, the engine will then parse the process definition files to an executable instance of this class, that can be used to start a <seealso cref="IProcessInstance"/>.
    /// 
    /// </summary>
    public interface IProcessDefinition
    {

        /// <summary>
        /// unique identifier </summary>
        string Id { get; }

        /// <summary>
        /// category name which is derived from the targetNamespace attribute in the definitions element
        /// </summary>
        string Category { get; }

        /// <summary>
        /// label used for display purposes </summary>
        string Name { get; }

        /// <summary>
        /// unique name for all versions this process definitions, bpmn process id </summary>
        string Key { get; }

        /// <summary>
        /// 业务键值
        /// </summary>
        string BusinessKey { get; }

        /// <summary>
        /// 业务路径
        /// </summary>
        string BusinessPath { get; }

        /// <summary>
        /// 启动表单
        /// </summary>
        string StartForm { get; }

        /// <summary>
        /// description of this process * </summary>
        string Description { get; }

        /// <summary>
        /// version of this process definition </summary>
        int Version { get; }

        /// <summary>
        /// name of <seealso cref="IRepositoryService#getResourceAsStream(String, String) the resource"/> of this process definition.
        /// </summary>
        string ResourceName { get; }

        /// <summary>
        /// The deployment in which this process definition is contained. </summary>
        string DeploymentId { get; }

        /// <summary>
        /// The resource name in the deployment of the diagram image (if any). </summary>
        string DiagramResourceName { get; }

        /// <summary>
        /// Does this process definition has a <seealso cref="FormService#getStartFormData(String) start form key"/>.
        /// </summary>
        bool HasStartFormKey { get; }

        /// <summary>
        /// Does this process definition has a graphical notation defined (such that a diagram can be generated)?
        /// </summary>
        bool IsGraphicalNotationDefined { get; }

        /// <summary>
        /// Returns true if the process definition is in suspended state. </summary>
        bool Suspended { get; }

        /// <summary>
        /// The tenant identifier of this process definition </summary>
        string TenantId { get; }

        /// <summary>
        /// The engine version for this process definition (5 or 6) </summary>
        string EngineVersion { get; }

    }

}