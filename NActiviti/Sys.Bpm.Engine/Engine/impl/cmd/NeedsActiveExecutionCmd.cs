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
    using Sys.Workflow.engine.runtime;
    using System.Collections.Generic;

    /// 
    [Serializable]
    public abstract class NeedsActiveExecutionCmd<T> : ICommand<T>
    {

        private const long serialVersionUID = 1L;

        protected internal string executionId;

        public NeedsActiveExecutionCmd(string executionId)
        {
            this.executionId = executionId;
        }

        public  virtual T  Execute(ICommandContext  commandContext)
        {
            if (executionId is null)
            {
                throw new ActivitiIllegalArgumentException("executionId is null");
            }

            IExecutionEntity execution = commandContext.ExecutionEntityManager.FindById<IExecutionEntity>(executionId);

            if (execution == null)
            {
                throw new ActivitiObjectNotFoundException("execution " + executionId + " doesn't exist", typeof(IExecution));
            }

            if (execution.Suspended)
            {
                throw new ActivitiException(SuspendedExceptionMessage);
            }

            return Execute(commandContext, execution);
        }

        /// <summary>
        /// Subclasses should implement this method. The provided <seealso cref="IExecutionEntity"/> is guaranteed to be active (ie. not suspended).
        /// </summary>
        protected  internal abstract T  Execute(ICommandContext  commandContext, IExecutionEntity execution);

        /// <summary>
        /// Subclasses can override this to provide a more detailed exception message that will be thrown when the execution is suspended.
        /// </summary>
        protected internal virtual string SuspendedExceptionMessage
        {
            get
            {
                return "Cannot execution operation because execution '" + executionId + "' is suspended";
            }
        }

    }

}