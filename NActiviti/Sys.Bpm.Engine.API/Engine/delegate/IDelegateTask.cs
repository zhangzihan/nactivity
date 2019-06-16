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
namespace Sys.Workflow.engine.@delegate
{

    using Sys.Workflow.bpmn.model;
    using Sys.Workflow.engine.impl.persistence.entity;
    using Sys.Workflow.engine.task;
    using Sys.Workflow;

    /// 
    public interface IDelegateTask : IVariableScope
    {

        /// <summary>
        /// DB id of the task. </summary>
        string Id { get; }

        /// <summary>
        /// Name or title of the task. </summary>
        string Name { get; set; }


        /// <summary>
        /// Free text description of the task. </summary>
        string Description { get; set; }


        /// <summary>
        /// indication of how important/urgent this task is with a number between 0 and 100 where higher values mean a higher priority and lower values mean lower priority: [0..19] lowest, [20..39] low,
        /// [40..59] normal, [60..79] high [80..100] highest
        /// </summary>
        int? Priority { get; set; }


        /// <summary>
        /// Reference to the process instance or null if it is not related to a process instance.
        /// </summary>
        string ProcessInstanceId { get; }

        /// <summary>
        /// Reference to the path of execution or null if it is not related to a process instance.
        /// </summary>
        string ExecutionId { get; }

        /// <summary>
        /// Reference to the process definition or null if it is not related to a process.
        /// </summary>
        string ProcessDefinitionId { get; }

        /// <summary>
        /// The date/time when this task was created </summary>
        DateTime? CreateTime { get; }

        /// <summary>
        /// The id of the activity in the process defining this task or null if this is not related to a process
        /// </summary>
        string TaskDefinitionKey { get; }

        /// <summary>
        /// Indicated whether this task is suspended or not. </summary>
        bool Suspended { get; }

        /// <summary>
        /// The tenant identifier of this task </summary>
        string TenantId { get; }

        /// <summary>
        /// The form key for the user task </summary>
        string FormKey { get; set; }


        /// <summary>
        /// Returns the execution currently at the task. </summary>
        IExecutionEntity Execution { get; }

        /// <summary>
        /// Returns the event name which triggered the task listener to fire for this task.
        /// </summary>
        string EventName { get; }

        /// <summary>
        /// 
        /// </summary>
        ActivitiListener CurrentActivitiListener { get; }

        /// <summary>
        /// The current <seealso cref="Sys.Workflow.engine.task.DelegationState"/> for this task.
        /// </summary>
        DelegationState? DelegationState { get; }

        /// <summary>
        /// Adds the given user as a candidate user to this task. </summary>
        void AddCandidateUser(string userId);

        /// <summary>
        /// Adds multiple users as candidate user to this task. </summary>
        void AddCandidateUsers(IEnumerable<string> candidateUsers);

        /// <summary>
        /// Adds the given group as candidate group to this task </summary>
        void AddCandidateGroup(string groupId);

        /// <summary>
        /// Adds multiple groups as candidate group to this task. </summary>
        void AddCandidateGroups(IEnumerable<string> candidateGroups);

        /// <summary>
        /// The <seealso cref="IUserInfo.Id"/> of the person responsible for this task. </summary>
        string Owner { get; set; }


        /// <summary>
        /// The <seealso cref="IUserInfo.Id"/> of the person to which this task is delegated.
        /// </summary>
        string Assignee { get; set; }


        /// <summary>
        /// The <seealso cref="IUserInfo.Id"/> of the person to which this task is delegated.
        /// </summary>
        string AssigneeUser { get; set; }


        /// <summary>
        /// Due date of the task. </summary>
        DateTime? DueDate { get; set; }


        /// <summary>
        /// The category of the task. This is an optional field and allows to 'tag' tasks as belonging to a certain category.
        /// </summary>
        string Category { get; set; }


        /// <summary>
        /// Involves a user with a task. The type of identity link is defined by the given identityLinkType.
        /// </summary>
        /// <param name="userId">
        ///          id of the user involve, cannot be null. </param>
        /// <param name="identityLinkType"> </param>
        ///          type of identityLink, cannot be null (<seealso cref="IdentityLinkType"/>)
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when the task or user doesn't exist. </exception>
        void AddUserIdentityLink(string userId, string identityLinkType);

        /// <summary>
        /// Involves a group with group task. The type of identityLink is defined by the given identityLink.
        /// </summary>
        /// <param name="groupId">
        ///          id of the group to involve, cannot be null. </param>
        /// <param name="identityLinkType"> </param>
        ///          type of identity, cannot be null (<seealso cref= <seealso cref="IdentityLinkType"/>). </seealso>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when the task or group doesn't exist. </exception>
        void AddGroupIdentityLink(string groupId, string identityLinkType);

        /// <summary>
        /// Convenience shorthand for <seealso cref="DeleteUserIdentityLink(String, String)"/> ; with type <seealso cref="IdentityLinkType.CANDIDATE"/>
        /// </summary>
        /// <param name="userId">
        ///          id of the user to use as candidate, cannot be null. </param>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when the task or user doesn't exist. </exception>
        void DeleteCandidateUser(string userId);

        /// <summary>
        /// Convenience shorthand for <seealso cref="DeleteGroupIdentityLink(String, String)"/>; with type <seealso cref="IdentityLinkType.CANDIDATE"/>
        /// </summary>
        /// <param name="groupId">
        ///          id of the group to use as candidate, cannot be null. </param>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when the task or group doesn't exist. </exception>
        void DeleteCandidateGroup(string groupId);

        /// <summary>
        /// Removes the association between a user and a task for the given identityLinkType.
        /// </summary>
        /// <param name="userId">
        ///          id of the user involve, cannot be null. </param>
        /// <param name="identityLinkType"> </param>
        ///          type of identityLink, cannot be null (<seealso cref="IdentityLinkType"/>).
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when the task or user doesn't exist. </exception>
        void DeleteUserIdentityLink(string userId, string identityLinkType);

        /// <summary>
        /// Removes the association between a group and a task for the given identityLinkType.
        /// </summary>
        /// <param name="groupId">
        ///          id of the group to involve, cannot be null. </param>
        /// <param name="identityLinkType"> </param>
        ///          type of identity, cannot be null (<seealso cref= <seealso cref="IdentityLinkType"/>). </seealso>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when the task or group doesn't exist. </exception>
        void DeleteGroupIdentityLink(string groupId, string identityLinkType);

        /// <summary>
        /// Retrieves the candidate users and groups associated with the task.
        /// </summary>
        /// <returns> set of <seealso cref="IIdentityLink"/>s of type <seealso cref="IdentityLinkType.CANDIDATE"/>. </returns>
        ISet<IIdentityLink> Candidates { get; }
    }

}