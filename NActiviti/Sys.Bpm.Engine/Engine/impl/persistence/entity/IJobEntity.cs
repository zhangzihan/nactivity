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

    /// <summary>
    /// Represents an async job: a piece of logic that needs to be executed asynchronously. 
    /// 
    /// 
    /// 
    /// </summary>
    public interface IJobEntity : IAbstractJobEntity
    {
        new string Id { get; set; }

        new string ExecutionId { get; set; }

        new string ProcessDefinitionId { get; set; }

        string LockOwner { get; set; }


        DateTime? LockExpirationTime { get; set; }

        new DateTime? Duedate { get; set; }
    }

}