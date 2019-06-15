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

namespace org.activiti.engine.task
{

    /// <summary>
    /// Any type of content that is be associated with a task or with a process instance.
    /// 
    /// 
    /// </summary>
    public interface IAttachment
    {

        /// <summary>
        /// unique id for this attachment </summary>
        string Id { get; }

        /// <summary>
        /// free user defined short (max 255 chars) name for this attachment </summary>
        string Name { get; set; }


        /// <summary>
        /// long (max 255 chars) explanation what this attachment is about in context of the task and/or process instance it's linked to.
        /// </summary>
        string Description { get; set; }


        /// <summary>
        /// indication of the type of content that this attachment refers to. Can be mime type or any other indication.
        /// </summary>
        string Type { get; }

        /// <summary>
        /// reference to the task to which this attachment is associated. </summary>
        string TaskId { get; }

        /// <summary>
        /// reference to the process instance to which this attachment is associated.
        /// </summary>
        string ProcessInstanceId { get; }

        /// <summary>
        /// the remote URL in case this is remote content. If the attachment content was {@link TaskService#createAttachment(String, String, String, String, String, java.io.InputStream) uploaded with an
        /// input stream}, then this method returns null and the content can be fetched with <seealso cref="ITaskService.GetAttachmentContent(String)"/>.
        /// </summary>
        string Url { get; }

        /// <summary>
        /// reference to the user who created this attachment. </summary>
        string UserId { get; }

        /// <summary>
        /// timestamp when this attachment was created </summary>
        DateTime Time { get; set; }


        /// <summary>
        /// the id of the byte array entity storing the content </summary>
        string ContentId { get; }

    }

}