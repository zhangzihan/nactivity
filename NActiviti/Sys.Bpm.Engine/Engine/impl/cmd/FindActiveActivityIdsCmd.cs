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

namespace Sys.Workflow.Engine.Impl.Cmd
{

    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Runtime;

    /// 
    /// 
    [Serializable]
    public class FindActiveActivityIdsCmd : ICommand<IList<string>>
    {

        private const long serialVersionUID = 1L;
        protected internal string executionId;

        public FindActiveActivityIdsCmd(string executionId)
        {
            this.executionId = executionId;
        }

        public virtual IList<string> Execute(ICommandContext commandContext)
        {
            if (executionId is null)
            {
                throw new ActivitiIllegalArgumentException("executionId is null");
            }

            IExecutionEntityManager executionEntityManager = commandContext.ExecutionEntityManager;
            IExecutionEntity execution = executionEntityManager.FindById<IExecutionEntity>(executionId);

            if (execution is null)
            {
                throw new ActivitiObjectNotFoundException("execution " + executionId + " doesn't exist", typeof(IExecution));
            }

            return FindActiveActivityIds(execution);
        }

        public virtual IList<string> FindActiveActivityIds(IExecutionEntity executionEntity)
        {
            IList<string> activeActivityIds = new List<string>();
            CollectActiveActivityIds(executionEntity, activeActivityIds);
            return activeActivityIds;
        }

        protected internal virtual void CollectActiveActivityIds(IExecutionEntity executionEntity, IList<string> activeActivityIds)
        {
            if (executionEntity.IsActive && !ReferenceEquals(executionEntity.ActivityId, null))
            {
                activeActivityIds.Add(executionEntity.ActivityId);
            }

            foreach (IExecutionEntity childExecution in executionEntity.Executions)
            {
                CollectActiveActivityIds(childExecution, activeActivityIds);
            }
        }

    }

}