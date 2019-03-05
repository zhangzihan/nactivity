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
namespace org.activiti.engine.impl.persistence.entity
{

    using org.activiti.engine.@event;

    /// <summary>
    /// An event log entry can only be inserted (and maybe deleted).
    /// 
    /// 
    /// </summary>
    public interface IEventLogEntryEntity : IEntity, IEventLogEntry
    {

        new long LogNumber { get; set; }

        new string Type { get; set; }

        new string ProcessDefinitionId { get; set; }

        new string ProcessInstanceId { get; set; }

        new string ExecutionId { get; set; }

        new string TaskId { get; set; }

        new DateTime? TimeStamp { get; set; }

        new string UserId { get; set; }

        new byte[] Data { get; set; }

        string LockOwner { get; set; }


        string LockTime { get; set; }


        int Processed { get; set; }


    }

}