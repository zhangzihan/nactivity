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
namespace Sys.Workflow.Engine
{

    /// <summary>
    /// Provides access to all the services that expose the BPM and workflow operations.
    /// 
    /// <ul>
    /// <li>
    /// <b><seealso cref="Sys.Workflow.Engine.IRuntimeService"/>: </b> Allows the creation of <seealso cref="Sys.Workflow.Engine.Repository.IDeployment"/>s and the starting of and searching on
    /// <seealso cref="Sys.Workflow.Engine.Runtime.IProcessInstance"/>s.</li>
    /// <li>
    /// <b><seealso cref="Sys.Workflow.Engine.ITaskService"/>: </b> Exposes operations to manage human (standalone) <seealso cref="Sys.Workflow.Engine.Tasks.ITask"/>s, such as claiming, completing and assigning tasks</li>
    /// <li>
    /// <b><seealso cref="Sys.Workflow.Engine.IdentityService"/>: </b> Used for managing <seealso cref="Sys.Workflow.Engine.identity.User"/>s, <seealso cref="Sys.Workflow.Engine.identity.Group"/>s and the relations between them<</li>
    /// <li>
    /// <b><seealso cref="Sys.Workflow.Engine.IManagementService"/>: </b> Exposes engine admin and maintenance operations</li>
    /// <li>
    /// <b><seealso cref="Sys.Workflow.Engine.IHistoryService"/>: </b> Service exposing information about ongoing and past process instances.</li>
    /// </ul>
    /// 
    /// Typically, there will be only one central ProcessEngine instance needed in a end-user application. Building a ProcessEngine is done through a <seealso cref="ProcessEngineConfiguration"/> instance and is a
    /// costly operation which should be avoided. For that purpose, it is advised to store it in a static field or JNDI location (or something similar). This is a thread-safe object, so no special
    /// precautions need to be taken.
    /// 
    /// </summary>
    public interface IProcessEngine
    {

        /// <summary>
        /// the version of the activiti library </summary>

        /// <summary>
        /// The name as specified in 'process-engine-name' in the activiti.cfg.xml configuration file. The default name for a process engine is 'default
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 关闭当前工作流引擎，释放所有资源
        /// </summary>
        void Close();

        IRepositoryService RepositoryService { get; }

        IRuntimeService RuntimeService { get; }

        ITaskService TaskService { get; }

        IHistoryService HistoryService { get; }

        IManagementService ManagementService { get; }

        IDynamicBpmnService DynamicBpmnService { get; }

        ProcessEngineConfiguration ProcessEngineConfiguration { get; }

    }
}