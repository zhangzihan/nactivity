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

namespace Sys.Workflow.engine.task
{

    /// <summary>
    /// Exposes twitter-like feeds for tasks and process instances.
    /// </summary>
    public interface IEvent
    {

        /// <summary>
        /// A user identity link was added with following message parts: [0] userId [1] identity link type (aka role)
        /// </summary>

        /// <summary>
        /// A user identity link was added with following message parts: [0] userId [1] identity link type (aka role)
        /// </summary>

        /// <summary>
        /// A group identity link was added with following message parts: [0] groupId [1] identity link type (aka role)
        /// </summary>

        /// <summary>
        /// A group identity link was added with following message parts: [0] groupId [1] identity link type (aka role)
        /// </summary>

        /// <summary>
        /// An user comment was added with the short version of the comment as message.
        /// </summary>

        /// <summary>
        /// An attachment was added with the attachment name as message. </summary>

        /// <summary>
        /// An attachment was deleted with the attachment name as message. </summary>

        /// <summary>
        /// Unique identifier for this event </summary>
        string Id { get; }

        /// <summary>
        /// Indicates the type of of action and also indicates the meaning of the parts as exposed in <seealso cref="MessageParts"/>
        /// </summary>
        string Action { get; }

        /// <summary>
        /// The meaning of the message parts is defined by the action as you can find in <seealso cref="Action"/>
        /// </summary>
        IList<string> MessageParts { get; }

        /// <summary>
        /// The message that can be used in case this action only has a single message part.
        /// </summary>
        string Message { get; }

        /// <summary>
        /// reference to the user that made the comment </summary>
        string UserId { get; }

        /// <summary>
        /// time and date when the user made the comment </summary>
        DateTime? Time { get; }

        /// <summary>
        /// reference to the task on which this comment was made </summary>
        string TaskId { get; }

        /// <summary>
        /// reference to the process instance on which this comment was made </summary>
        string ProcessInstanceId { get; }

    }

    /// <summary>
    /// 
    /// </summary>
    public static class EventFields
    {
        /// <summary>
        /// 
        /// </summary>
        public const string ACTION_ADD_USER_LINK = "AddUserLink";
        /// <summary>
        /// 
        /// </summary>
        public const string ACTION_DELETE_USER_LINK = "DeleteUserLink";
        /// <summary>
        /// 
        /// </summary>
        public const string ACTION_ADD_GROUP_LINK = "AddGroupLink";
        /// <summary>
        /// 
        /// </summary>
        public const string ACTION_DELETE_GROUP_LINK = "DeleteGroupLink";
        /// <summary>
        /// 
        /// </summary>
        public const string ACTION_ADD_COMMENT = "AddComment";
        /// <summary>
        /// 
        /// </summary>
        public const string ACTION_ADD_ATTACHMENT = "AddAttachment";
        /// <summary>
        /// 
        /// </summary>
        public const string ACTION_DELETE_ATTACHMENT = "DeleteAttachment";
    }
}