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

namespace org.activiti.engine
{

    using org.activiti.engine.history;

    /// <summary>
    /// Service exposing information about ongoing and past process instances. This is different from the runtime information in the sense that this runtime information only contains the actual runtime
    /// state at any given moment and it is optimized for runtime process execution performance. The history information is optimized for easy querying and remains permanent in the persistent storage.
    /// 
    /// 
    /// 
    /// 
    /// </summary>
    public interface IHistoryService
    {

        /// <summary>
        /// Creates a new programmatic query to search for <seealso cref="IHistoricProcessInstance"/>s.
        /// </summary>
        IHistoricProcessInstanceQuery CreateHistoricProcessInstanceQuery();

        /// <summary>
        /// Creates a new programmatic query to search for <seealso cref="IHistoricActivityInstance"/>s.
        /// </summary>
        IHistoricActivityInstanceQuery CreateHistoricActivityInstanceQuery();

        /// <summary>
        /// Creates a new programmatic query to search for <seealso cref="IHistoricTaskInstance"/>s.
        /// </summary>
        IHistoricTaskInstanceQuery CreateHistoricTaskInstanceQuery();

        /// <summary>
        /// Creates a new programmatic query to search for <seealso cref="IHistoricDetail"/>s. </summary>
        IHistoricDetailQuery CreateHistoricDetailQuery();

        /// <summary>
        /// Returns a new <seealso cref="org.activiti.engine.query.NativeQuery"/> for process definitions.
        /// </summary>
        INativeHistoricDetailQuery CreateNativeHistoricDetailQuery();

        /// <summary>
        /// Creates a new programmatic query to search for <seealso cref="IHistoricVariableInstance"/>s.
        /// </summary>
        IHistoricVariableInstanceQuery CreateHistoricVariableInstanceQuery();

        /// <summary>
        /// Returns a new <seealso cref="org.activiti.engine.query.NativeQuery"/> for process definitions.
        /// </summary>
        INativeHistoricVariableInstanceQuery CreateNativeHistoricVariableInstanceQuery();

        /// <summary>
        /// Deletes historic task instance. This might be useful for tasks that are <seealso cref="ITaskService#newTask() dynamically created"/> and then <seealso cref="ITaskService#complete(String) completed"/>. If the historic
        /// task instance doesn't exist, no exception is thrown and the method returns normal.
        /// </summary>
        void DeleteHistoricTaskInstance(string taskId);

        /// <summary>
        /// Deletes historic process instance. All historic activities, historic task and historic details (variable updates, form properties) are deleted as well.
        /// </summary>
        void DeleteHistoricProcessInstance(string processInstanceId);

        /// <summary>
        /// creates a native query to search for <seealso cref="IHistoricProcessInstance"/>s via SQL
        /// </summary>
        INativeHistoricProcessInstanceQuery CreateNativeHistoricProcessInstanceQuery();

        /// <summary>
        /// creates a native query to search for <seealso cref="IHistoricTaskInstance"/>s via SQL
        /// </summary>
        INativeHistoricTaskInstanceQuery CreateNativeHistoricTaskInstanceQuery();

        /// <summary>
        /// creates a native query to search for <seealso cref="IHistoricActivityInstance"/>s via SQL
        /// </summary>
        INativeHistoricActivityInstanceQuery CreateNativeHistoricActivityInstanceQuery();

        /// <summary>
        /// Retrieves the <seealso cref="IHistoricIdentityLink"/>s associated with the given task. Such an <seealso cref="IdentityLink"/> informs how a certain identity (eg. group or user) is associated with a certain task (eg.
        /// as candidate, assignee, etc.), even if the task is completed as opposed to <seealso cref="IdentityLink"/>s which only exist for active tasks.
        /// </summary>
        IList<IHistoricIdentityLink> GetHistoricIdentityLinksForTask(string taskId);

        /// <summary>
        /// Retrieves the <seealso cref="IHistoricIdentityLink"/>s associated with the given process instance. Such an <seealso cref="IdentityLink"/> informs how a certain identity (eg. group or user) is associated with a
        /// certain process instance, even if the instance is completed as opposed to <seealso cref="IdentityLink"/>s which only exist for active instances.
        /// </summary>
        IList<IHistoricIdentityLink> GetHistoricIdentityLinksForProcessInstance(string processInstanceId);

        /// <summary>
        /// Allows to retrieve the <seealso cref="IProcessInstanceHistoryLog"/> for one process instance.
        /// </summary>
        IProcessInstanceHistoryLogQuery CreateProcessInstanceHistoryLogQuery(string processInstanceId);
    }
}