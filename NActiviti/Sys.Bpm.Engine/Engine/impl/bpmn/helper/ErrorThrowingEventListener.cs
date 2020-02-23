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
namespace Sys.Workflow.Engine.Impl.Bpmn.Helper
{
    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using System;

    /// <summary>
    /// An <seealso cref="IActivitiEventListener"/> that throws a error event when an event is dispatched to it.
    /// 
    /// 
    /// 
    /// </summary>
    public class ErrorThrowingEventListener : BaseDelegateEventListener
    {

        protected internal string errorCode;

        public override void OnEvent(IActivitiEvent @event)
        {
            if (IsValidEvent(@event))
            {
                var commandContext = Context.CommandContext;

                IExecutionEntity execution = null;
                if (@event.ExecutionId is object)
                {
                    // Get the execution based on the event's execution ID instead 
                    execution = commandContext.ExecutionEntityManager.FindById<IExecutionEntity>(@event.ExecutionId);
                }

                if (execution is null)
                {
                    throw new ActivitiException("No execution context active and event is not related to an execution. No compensation event can be thrown.");
                }

                try
                {
                    ErrorPropagation.PropagateError(errorCode, execution);
                }
                catch (Exception e)
                {
                    throw new ActivitiException("Error while propagating error-event", e);
                }
            }
        }

        public virtual string ErrorCode
        {
            set
            {
                this.errorCode = value;
            }
        }

        public override bool FailOnException
        {
            get
            {
                return true;
            }
        }
    }

}