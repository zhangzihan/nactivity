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
namespace Sys.Workflow.engine.impl.cmd
{
    using Sys.Workflow.engine.impl.interceptor;
    using Sys.Workflow.engine.impl.persistence.entity;

    /// 
    /// 
    [Serializable]
    public class GetExecutionByIdCmd : ICommand<IExecutionEntity>
    {

        private const long serialVersionUID = 1L;
        protected internal string executionId;

        public GetExecutionByIdCmd(string executionId)
        {
            this.executionId = executionId;
        }

        public virtual IExecutionEntity Execute(ICommandContext commandContext)
        {
            if (executionId is null)
            {
                throw new ActivitiIllegalArgumentException("executionId is null");
            }

            IExecutionEntity execution = commandContext.ExecutionEntityManager.FindById<IExecutionEntity>(executionId);

            return execution;
        }
    }
}