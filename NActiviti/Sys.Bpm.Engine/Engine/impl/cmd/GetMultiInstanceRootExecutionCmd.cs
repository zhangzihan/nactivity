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
namespace Sys.Workflow.Engine.Impl.Cmd
{
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;

    /// 
    /// 
    [Serializable]
    public class GetMultiInstanceRootExecutionCmd : ICommand<IExecutionEntity>
    {
        private const long serialVersionUID = 1L;
        protected internal IExecutionEntity execution;

        public GetMultiInstanceRootExecutionCmd(IExecutionEntity execution)
        {
            this.execution = execution;
        }

        public virtual IExecutionEntity Execute(ICommandContext commandContext)
        {
            IExecutionEntity multiInstanceRootExecution = null;
            IExecutionEntity currentExecution = execution;
            while (currentExecution is object && multiInstanceRootExecution is null && currentExecution.Parent is object)
            {
                if (currentExecution.IsMultiInstanceRoot)
                {
                    multiInstanceRootExecution = currentExecution;
                }
                else
                {
                    currentExecution = currentExecution.Parent;
                }
            }
            return multiInstanceRootExecution;
        }
    }
}