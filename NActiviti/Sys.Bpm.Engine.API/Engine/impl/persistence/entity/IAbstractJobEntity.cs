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
namespace Sys.Workflow.Engine.Impl.Persistence.Entity
{

    using Sys.Workflow.Engine.Impl.DB;
    using Sys.Workflow.Engine.Runtime;

    /// 
    /// 
    public interface IAbstractJobEntity : IJob, IEntity, IHasRevision
    {
        new string Id { get; set; }

        IExecutionEntity Execution { set; }

        new DateTime? Duedate { get; set; }

        new string ExecutionId { get; set; }

        new int Retries { get; set; }

        new string ProcessInstanceId { get; set; }

        new bool Exclusive { get; set; }


        new string ProcessDefinitionId { get; set; }

        new string JobHandlerType { get; set; }


        new string JobHandlerConfiguration { get; set; }


        new string JobType { get; set; }


        string Repeat { get; set; }


        DateTime? EndDate { get; set; }


        int MaxIterations { get; set; }


        string ExceptionStacktrace { get; set; }


        new string ExceptionMessage { get; set; }

        IByteArrayRef ExceptionByteArrayRef { get; }

        new string TenantId { get; set; }

    }

}