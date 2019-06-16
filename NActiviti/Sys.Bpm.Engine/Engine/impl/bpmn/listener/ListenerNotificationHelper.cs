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
namespace Sys.Workflow.Engine.Impl.Bpmn.Listeners
{

    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine.Delegate;
    using Sys.Workflow.Engine.Impl.Bpmn.Parser.Factory;
    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Delegate.Invocation;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Impl.Util;

    /// 
    public class ListenerNotificationHelper
    {

        public virtual void ExecuteExecutionListeners(IHasExecutionListeners elementWithExecutionListeners, IExecutionEntity execution, string eventType)
        {
            IList<ActivitiListener> listeners = elementWithExecutionListeners.ExecutionListeners;
            if (listeners != null && listeners.Count > 0)
            {
                IListenerFactory listenerFactory = Context.ProcessEngineConfiguration.ListenerFactory;
                foreach (ActivitiListener activitiListener in listeners)
                {

                    if (eventType.Equals(activitiListener.Event))
                    {

                        IBaseExecutionListener executionListener = null;

                        if (ImplementationType.IMPLEMENTATION_TYPE_CLASS.Equals(activitiListener.ImplementationType, StringComparison.CurrentCultureIgnoreCase))
                        {
                            executionListener = listenerFactory.CreateClassDelegateExecutionListener(activitiListener);
                        }
                        else if (ImplementationType.IMPLEMENTATION_TYPE_EXPRESSION.Equals(activitiListener.ImplementationType, StringComparison.CurrentCultureIgnoreCase))
                        {
                            executionListener = listenerFactory.CreateExpressionExecutionListener(activitiListener);
                        }
                        else if (ImplementationType.IMPLEMENTATION_TYPE_DELEGATEEXPRESSION.Equals(activitiListener.ImplementationType, StringComparison.CurrentCultureIgnoreCase))
                        {
                            if (!(activitiListener.OnTransaction is null))
                            {
                                executionListener = listenerFactory.CreateTransactionDependentDelegateExpressionExecutionListener(activitiListener);
                            }
                            else
                            {
                                executionListener = listenerFactory.CreateDelegateExpressionExecutionListener(activitiListener);
                            }
                        }
                        else if (ImplementationType.IMPLEMENTATION_TYPE_INSTANCE.Equals(activitiListener.ImplementationType, StringComparison.CurrentCultureIgnoreCase))
                        {
                            executionListener = (IExecutionListener)activitiListener.Instance;
                        }

                        if (executionListener != null)
                        {
                            if (!(activitiListener.OnTransaction is null))
                            {
                                PlanTransactionDependentExecutionListener(listenerFactory, execution, (ITransactionDependentExecutionListener)executionListener, activitiListener);
                            }
                            else
                            {
                                execution.EventName = eventType; // eventName is used to differentiate the event when reusing an execution listener for various events
                                execution.CurrentActivitiListener = activitiListener;
                                ((IExecutionListener)executionListener).Notify(execution);
                                execution.EventName = null;
                                execution.CurrentActivitiListener = null;
                            }
                        }
                    }
                }
            }
        }

        protected internal virtual void PlanTransactionDependentExecutionListener(IListenerFactory listenerFactory, IExecutionEntity execution, ITransactionDependentExecutionListener executionListener, ActivitiListener activitiListener)
        {
            IDictionary<string, object> executionVariablesToUse = execution.Variables;
            ICustomPropertiesResolver customPropertiesResolver = CreateCustomPropertiesResolver(activitiListener);
            IDictionary<string, object> customPropertiesMapToUse = InvokeCustomPropertiesResolver(execution, customPropertiesResolver);

            TransactionDependentExecutionListenerExecutionScope scope = new TransactionDependentExecutionListenerExecutionScope(execution.ProcessInstanceId, execution.Id, execution.CurrentFlowElement, executionVariablesToUse, customPropertiesMapToUse);

            AddTransactionListener(activitiListener, new ExecuteExecutionListenerTransactionListener(executionListener, scope));
        }

        public virtual void ExecuteTaskListeners(ITaskEntity taskEntity, string eventType)
        {
            if (!(taskEntity.ProcessDefinitionId is null))
            {
                Process process = ProcessDefinitionUtil.GetProcess(taskEntity.ProcessDefinitionId);
                FlowElement flowElement = process.GetFlowElement(taskEntity.TaskDefinitionKey, true);
                if (flowElement is UserTask userTask)
                {
                    ExecuteTaskListeners(userTask, taskEntity, eventType);
                }
            }
        }

        public virtual void ExecuteTaskListeners(UserTask userTask, ITaskEntity taskEntity, string eventType)
        {
            foreach (ActivitiListener activitiListener in userTask.TaskListeners)
            {
                string @event = activitiListener.Event;
                if (@event.Equals(eventType) || @event.Equals(BaseTaskListenerFields.EVENTNAME_ALL_EVENTS))
                {
                    IBaseTaskListener taskListener = CreateTaskListener(activitiListener);

                    if (!(activitiListener.OnTransaction is null))
                    {
                        PlanTransactionDependentTaskListener(taskEntity.Execution, (ITransactionDependentTaskListener)taskListener, activitiListener);
                    }
                    else
                    {
                        taskEntity.EventName = eventType;
                        taskEntity.CurrentActivitiListener = activitiListener;
                        try
                        {
                            Context.ProcessEngineConfiguration.DelegateInterceptor.HandleInvocation(new TaskListenerInvocation((ITaskListener)taskListener, taskEntity));
                        }
                        catch (Exception e)
                        {
                            throw new ActivitiException("Exception while invoking TaskListener: " + e.Message, e);
                        }
                        finally
                        {
                            taskEntity.EventName = null;
                            taskEntity.CurrentActivitiListener = null;
                        }
                    }
                }
            }
        }

        protected internal virtual IBaseTaskListener CreateTaskListener(ActivitiListener activitiListener)
        {
            IBaseTaskListener taskListener = null;

            IListenerFactory listenerFactory = Context.ProcessEngineConfiguration.ListenerFactory;
            if (ImplementationType.IMPLEMENTATION_TYPE_CLASS.Equals(activitiListener.ImplementationType, StringComparison.CurrentCultureIgnoreCase))
            {
                taskListener = listenerFactory.CreateClassDelegateTaskListener(activitiListener);
            }
            else if (ImplementationType.IMPLEMENTATION_TYPE_EXPRESSION.Equals(activitiListener.ImplementationType, StringComparison.CurrentCultureIgnoreCase))
            {
                taskListener = listenerFactory.CreateExpressionTaskListener(activitiListener);
            }
            else if (ImplementationType.IMPLEMENTATION_TYPE_DELEGATEEXPRESSION.Equals(activitiListener.ImplementationType, StringComparison.CurrentCultureIgnoreCase))
            {
                if (!(activitiListener.OnTransaction is null))
                {
                    taskListener = listenerFactory.CreateTransactionDependentDelegateExpressionTaskListener(activitiListener);
                }
                else
                {
                    taskListener = listenerFactory.CreateDelegateExpressionTaskListener(activitiListener);
                }
            }
            else if (ImplementationType.IMPLEMENTATION_TYPE_INSTANCE.Equals(activitiListener.ImplementationType, StringComparison.CurrentCultureIgnoreCase))
            {
                taskListener = (ITaskListener)activitiListener.Instance;
            }
            return taskListener;
        }

        protected internal virtual void PlanTransactionDependentTaskListener(IExecutionEntity execution, ITransactionDependentTaskListener taskListener, ActivitiListener activitiListener)
        {
            IDictionary<string, object> executionVariablesToUse = execution.Variables;
            ICustomPropertiesResolver customPropertiesResolver = CreateCustomPropertiesResolver(activitiListener);
            IDictionary<string, object> customPropertiesMapToUse = InvokeCustomPropertiesResolver(execution, customPropertiesResolver);

            TransactionDependentTaskListenerExecutionScope scope = new TransactionDependentTaskListenerExecutionScope(execution.ProcessInstanceId, execution.Id, (TaskActivity)execution.CurrentFlowElement, executionVariablesToUse, customPropertiesMapToUse);
            AddTransactionListener(activitiListener, new ExecuteTaskListenerTransactionListener(taskListener, scope));
        }

        protected internal virtual ICustomPropertiesResolver CreateCustomPropertiesResolver(ActivitiListener activitiListener)
        {
            ICustomPropertiesResolver customPropertiesResolver = null;
            IListenerFactory listenerFactory = Context.ProcessEngineConfiguration.ListenerFactory;
            if (ImplementationType.IMPLEMENTATION_TYPE_CLASS.Equals(activitiListener.CustomPropertiesResolverImplementationType, StringComparison.CurrentCultureIgnoreCase))
            {
                customPropertiesResolver = listenerFactory.CreateClassDelegateCustomPropertiesResolver(activitiListener);
            }
            else if (ImplementationType.IMPLEMENTATION_TYPE_EXPRESSION.Equals(activitiListener.CustomPropertiesResolverImplementationType, StringComparison.CurrentCultureIgnoreCase))
            {
                customPropertiesResolver = listenerFactory.CreateExpressionCustomPropertiesResolver(activitiListener);
            }
            else if (ImplementationType.IMPLEMENTATION_TYPE_DELEGATEEXPRESSION.Equals(activitiListener.CustomPropertiesResolverImplementationType, StringComparison.CurrentCultureIgnoreCase))
            {
                customPropertiesResolver = listenerFactory.CreateDelegateExpressionCustomPropertiesResolver(activitiListener);
            }
            return customPropertiesResolver;
        }

        protected internal virtual IDictionary<string, object> InvokeCustomPropertiesResolver(IExecutionEntity execution, ICustomPropertiesResolver customPropertiesResolver)
        {
            IDictionary<string, object> customPropertiesMapToUse = null;
            if (customPropertiesResolver != null)
            {
                customPropertiesMapToUse = customPropertiesResolver.GetCustomPropertiesMap(execution);
            }
            return customPropertiesMapToUse;
        }

        protected internal virtual void AddTransactionListener(ActivitiListener activitiListener, ITransactionListener transactionListener)
        {
            ITransactionContext transactionContext = Context.TransactionContext;
            if (TransactionDependentExecutionListenerFields.ON_TRANSACTION_BEFORE_COMMIT.Equals(activitiListener.OnTransaction))
            {
                transactionContext.AddTransactionListener(TransactionState.COMMITTING, transactionListener);
            }
            else if (TransactionDependentExecutionListenerFields.ON_TRANSACTION_COMMITTED.Equals(activitiListener.OnTransaction))
            {
                transactionContext.AddTransactionListener(TransactionState.COMMITTED, transactionListener);
            }
            else if (TransactionDependentExecutionListenerFields.ON_TRANSACTION_ROLLED_BACK.Equals(activitiListener.OnTransaction))
            {
                transactionContext.AddTransactionListener(TransactionState.ROLLED_BACK, transactionListener);
            }
        }
    }
}