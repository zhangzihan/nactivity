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
namespace org.activiti.engine.impl.bpmn.listener
{

    using org.activiti.bpmn.model;
    using org.activiti.engine.@delegate;
    using org.activiti.engine.impl.bpmn.parser.factory;
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.@delegate.invocation;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;

    /// 
    public class ListenerNotificationHelper
    {

        public virtual void executeExecutionListeners(IHasExecutionListeners elementWithExecutionListeners, IExecutionEntity execution, string eventType)
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
                            executionListener = listenerFactory.createClassDelegateExecutionListener(activitiListener);
                        }
                        else if (ImplementationType.IMPLEMENTATION_TYPE_EXPRESSION.Equals(activitiListener.ImplementationType, StringComparison.CurrentCultureIgnoreCase))
                        {
                            executionListener = listenerFactory.createExpressionExecutionListener(activitiListener);
                        }
                        else if (ImplementationType.IMPLEMENTATION_TYPE_DELEGATEEXPRESSION.Equals(activitiListener.ImplementationType, StringComparison.CurrentCultureIgnoreCase))
                        {
                            if (!ReferenceEquals(activitiListener.OnTransaction, null))
                            {
                                executionListener = listenerFactory.createTransactionDependentDelegateExpressionExecutionListener(activitiListener);
                            }
                            else
                            {
                                executionListener = listenerFactory.createDelegateExpressionExecutionListener(activitiListener);
                            }
                        }
                        else if (ImplementationType.IMPLEMENTATION_TYPE_INSTANCE.Equals(activitiListener.ImplementationType, StringComparison.CurrentCultureIgnoreCase))
                        {
                            executionListener = (IExecutionListener)activitiListener.Instance;
                        }

                        if (executionListener != null)
                        {
                            if (!ReferenceEquals(activitiListener.OnTransaction, null))
                            {
                                planTransactionDependentExecutionListener(listenerFactory, execution, (ITransactionDependentExecutionListener)executionListener, activitiListener);
                            }
                            else
                            {
                                execution.EventName = eventType; // eventName is used to differentiate the event when reusing an execution listener for various events
                                execution.CurrentActivitiListener = activitiListener;
                                ((IExecutionListener)executionListener).notify(execution);
                                execution.EventName = null;
                                execution.CurrentActivitiListener = null;
                            }
                        }
                    }
                }
            }
        }

        protected internal virtual void planTransactionDependentExecutionListener(IListenerFactory listenerFactory, IExecutionEntity execution, ITransactionDependentExecutionListener executionListener, ActivitiListener activitiListener)
        {
            IDictionary<string, object> executionVariablesToUse = execution.Variables;
            ICustomPropertiesResolver customPropertiesResolver = createCustomPropertiesResolver(activitiListener);
            IDictionary<string, object> customPropertiesMapToUse = invokeCustomPropertiesResolver(execution, customPropertiesResolver);

            TransactionDependentExecutionListenerExecutionScope scope = new TransactionDependentExecutionListenerExecutionScope(execution.ProcessInstanceId, execution.Id, execution.CurrentFlowElement, executionVariablesToUse, customPropertiesMapToUse);

            addTransactionListener(activitiListener, new ExecuteExecutionListenerTransactionListener(executionListener, scope));
        }

        public virtual void executeTaskListeners(ITaskEntity taskEntity, string eventType)
        {
            if (!ReferenceEquals(taskEntity.ProcessDefinitionId, null))
            {
                org.activiti.bpmn.model.Process process = ProcessDefinitionUtil.getProcess(taskEntity.ProcessDefinitionId);
                FlowElement flowElement = process.getFlowElement(taskEntity.TaskDefinitionKey, true);
                if (flowElement is UserTask)
                {
                    UserTask userTask = (UserTask)flowElement;
                    executeTaskListeners(userTask, taskEntity, eventType);
                }
            }
        }

        public virtual void executeTaskListeners(UserTask userTask, ITaskEntity taskEntity, string eventType)
        {
            foreach (ActivitiListener activitiListener in userTask.TaskListeners)
            {
                string @event = activitiListener.Event;
                if (@event.Equals(eventType) || @event.Equals(BaseTaskListener_Fields.EVENTNAME_ALL_EVENTS))
                {
                    IBaseTaskListener taskListener = createTaskListener(activitiListener);

                    if (!ReferenceEquals(activitiListener.OnTransaction, null))
                    {
                        planTransactionDependentTaskListener(taskEntity.Execution, (ITransactionDependentTaskListener)taskListener, activitiListener);
                    }
                    else
                    {
                        taskEntity.EventName = eventType;
                        taskEntity.CurrentActivitiListener = activitiListener;
                        try
                        {
                            Context.ProcessEngineConfiguration.DelegateInterceptor.handleInvocation(new TaskListenerInvocation((ITaskListener)taskListener, taskEntity));
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

        protected internal virtual IBaseTaskListener createTaskListener(ActivitiListener activitiListener)
        {
            IBaseTaskListener taskListener = null;

            IListenerFactory listenerFactory = Context.ProcessEngineConfiguration.ListenerFactory;
            if (ImplementationType.IMPLEMENTATION_TYPE_CLASS.Equals(activitiListener.ImplementationType, StringComparison.CurrentCultureIgnoreCase))
            {
                taskListener = listenerFactory.createClassDelegateTaskListener(activitiListener);
            }
            else if (ImplementationType.IMPLEMENTATION_TYPE_EXPRESSION.Equals(activitiListener.ImplementationType, StringComparison.CurrentCultureIgnoreCase))
            {
                taskListener = listenerFactory.createExpressionTaskListener(activitiListener);
            }
            else if (ImplementationType.IMPLEMENTATION_TYPE_DELEGATEEXPRESSION.Equals(activitiListener.ImplementationType, StringComparison.CurrentCultureIgnoreCase))
            {
                if (!ReferenceEquals(activitiListener.OnTransaction, null))
                {
                    taskListener = listenerFactory.createTransactionDependentDelegateExpressionTaskListener(activitiListener);
                }
                else
                {
                    taskListener = listenerFactory.createDelegateExpressionTaskListener(activitiListener);
                }
            }
            else if (ImplementationType.IMPLEMENTATION_TYPE_INSTANCE.Equals(activitiListener.ImplementationType, StringComparison.CurrentCultureIgnoreCase))
            {
                taskListener = (ITaskListener)activitiListener.Instance;
            }
            return taskListener;
        }

        protected internal virtual void planTransactionDependentTaskListener(IExecutionEntity execution, ITransactionDependentTaskListener taskListener, ActivitiListener activitiListener)
        {
            IDictionary<string, object> executionVariablesToUse = execution.Variables;
            ICustomPropertiesResolver customPropertiesResolver = createCustomPropertiesResolver(activitiListener);
            IDictionary<string, object> customPropertiesMapToUse = invokeCustomPropertiesResolver(execution, customPropertiesResolver);

            TransactionDependentTaskListenerExecutionScope scope = new TransactionDependentTaskListenerExecutionScope(execution.ProcessInstanceId, execution.Id, (Task)execution.CurrentFlowElement, executionVariablesToUse, customPropertiesMapToUse);
            addTransactionListener(activitiListener, new ExecuteTaskListenerTransactionListener(taskListener, scope));
        }

        protected internal virtual ICustomPropertiesResolver createCustomPropertiesResolver(ActivitiListener activitiListener)
        {
            ICustomPropertiesResolver customPropertiesResolver = null;
            IListenerFactory listenerFactory = Context.ProcessEngineConfiguration.ListenerFactory;
            if (ImplementationType.IMPLEMENTATION_TYPE_CLASS.Equals(activitiListener.CustomPropertiesResolverImplementationType, StringComparison.CurrentCultureIgnoreCase))
            {
                customPropertiesResolver = listenerFactory.createClassDelegateCustomPropertiesResolver(activitiListener);
            }
            else if (ImplementationType.IMPLEMENTATION_TYPE_EXPRESSION.Equals(activitiListener.CustomPropertiesResolverImplementationType, StringComparison.CurrentCultureIgnoreCase))
            {
                customPropertiesResolver = listenerFactory.createExpressionCustomPropertiesResolver(activitiListener);
            }
            else if (ImplementationType.IMPLEMENTATION_TYPE_DELEGATEEXPRESSION.Equals(activitiListener.CustomPropertiesResolverImplementationType, StringComparison.CurrentCultureIgnoreCase))
            {
                customPropertiesResolver = listenerFactory.createDelegateExpressionCustomPropertiesResolver(activitiListener);
            }
            return customPropertiesResolver;
        }

        protected internal virtual IDictionary<string, object> invokeCustomPropertiesResolver(IExecutionEntity execution, ICustomPropertiesResolver customPropertiesResolver)
        {
            IDictionary<string, object> customPropertiesMapToUse = null;
            if (customPropertiesResolver != null)
            {
                customPropertiesMapToUse = customPropertiesResolver.getCustomPropertiesMap(execution);
            }
            return customPropertiesMapToUse;
        }

        protected internal virtual void addTransactionListener(ActivitiListener activitiListener, ITransactionListener transactionListener)
        {
            ITransactionContext transactionContext = Context.TransactionContext;
            if (TransactionDependentExecutionListener_Fields.ON_TRANSACTION_BEFORE_COMMIT.Equals(activitiListener.OnTransaction))
            {
                transactionContext.addTransactionListener(TransactionState.COMMITTING, transactionListener);

            }
            else if (TransactionDependentExecutionListener_Fields.ON_TRANSACTION_COMMITTED.Equals(activitiListener.OnTransaction))
            {
                transactionContext.addTransactionListener(TransactionState.COMMITTED, transactionListener);

            }
            else if (TransactionDependentExecutionListener_Fields.ON_TRANSACTION_ROLLED_BACK.Equals(activitiListener.OnTransaction))
            {
                transactionContext.addTransactionListener(TransactionState.ROLLED_BACK, transactionListener);

            }
        }

    }

}