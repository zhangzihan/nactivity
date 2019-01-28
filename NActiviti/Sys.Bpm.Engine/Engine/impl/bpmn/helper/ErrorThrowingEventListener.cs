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
namespace org.activiti.engine.impl.bpmn.helper
{
    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
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

        public override void onEvent(IActivitiEvent @event)
        {
            if (isValidEvent(@event))
            {
                ICommandContext commandContext = Context.CommandContext;

                IExecutionEntity execution = null;

                if (!ReferenceEquals(@event.ExecutionId, null))
                {
                    // Get the execution based on the event's execution ID instead execution = Context.CommandContext.ExecutionEntityManager.findById<instead>(new KeyValuePair<string, object>("id", @event.ExecutionId));
                }

                if (execution == null)
                {
                    throw new ActivitiException("No execution context active and event is not related to an execution. No compensation event can be thrown.");
                }

                try
                {
                    ErrorPropagation.propagateError(errorCode, execution);
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