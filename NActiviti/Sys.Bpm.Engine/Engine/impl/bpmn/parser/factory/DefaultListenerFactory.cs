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
namespace org.activiti.engine.impl.bpmn.parser.factory
{

    using org.activiti.bpmn.model;
    using org.activiti.engine.@delegate;
    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.impl.bpmn.helper;
    using org.activiti.engine.impl.bpmn.listener;
    using org.activiti.engine.repository;
    using org.activiti.engine.runtime;
    using org.activiti.engine.task;

    /// <summary>
    /// Default implementation of the <seealso cref="IListenerFactory"/>. Used when no custom <seealso cref="IListenerFactory"/> is injected on the <seealso cref="ProcessEngineConfigurationImpl"/>.
    /// 
    /// 
    /// </summary>
    public class DefaultListenerFactory : AbstractBehaviorFactory, IListenerFactory
    {
        private readonly IClassDelegateFactory classDelegateFactory;

        public DefaultListenerFactory(IClassDelegateFactory classDelegateFactory)
        {
            this.classDelegateFactory = classDelegateFactory;
        }

        public DefaultListenerFactory() : this(new DefaultClassDelegateFactory())
        {
        }

        public static readonly IDictionary<string, Type> ENTITY_MAPPING = new Dictionary<string, Type>();
        static DefaultListenerFactory()
        {
            ENTITY_MAPPING["attachment"] = typeof(IAttachment);
            ENTITY_MAPPING["comment"] = typeof(IComment);
            ENTITY_MAPPING["execution"] = typeof(IExecution);
            ENTITY_MAPPING["identity-link"] = typeof(IIdentityLink);
            ENTITY_MAPPING["job"] = typeof(IJob);
            ENTITY_MAPPING["process-definition"] = typeof(IProcessDefinition);
            ENTITY_MAPPING["process-instance"] = typeof(IProcessInstance);
            ENTITY_MAPPING["task"] = typeof(ITask);
        }

        public virtual ITaskListener CreateClassDelegateTaskListener(ActivitiListener activitiListener)
        {
            return classDelegateFactory.Create(activitiListener.Implementation, CreateFieldDeclarations(activitiListener.FieldExtensions));
        }

        public virtual ITaskListener CreateExpressionTaskListener(ActivitiListener activitiListener)
        {
            return new ExpressionTaskListener(expressionManager.CreateExpression(activitiListener.Implementation));
        }

        public virtual ITaskListener CreateDelegateExpressionTaskListener(ActivitiListener activitiListener)
        {
            return new DelegateExpressionTaskListener(expressionManager.CreateExpression(activitiListener.Implementation), CreateFieldDeclarations(activitiListener.FieldExtensions));
        }

        public virtual ITransactionDependentTaskListener CreateTransactionDependentDelegateExpressionTaskListener(ActivitiListener activitiListener)
        {
            return new DelegateExpressionTransactionDependentTaskListener(expressionManager.CreateExpression(activitiListener.Implementation));
        }

        public virtual IExecutionListener CreateClassDelegateExecutionListener(ActivitiListener activitiListener)
        {
            return classDelegateFactory.Create(activitiListener.Implementation, CreateFieldDeclarations(activitiListener.FieldExtensions));
        }

        public virtual IExecutionListener CreateExpressionExecutionListener(ActivitiListener activitiListener)
        {
            return new ExpressionExecutionListener(expressionManager.CreateExpression(activitiListener.Implementation));
        }

        public virtual IExecutionListener CreateDelegateExpressionExecutionListener(ActivitiListener activitiListener)
        {
            return new DelegateExpressionExecutionListener(expressionManager.CreateExpression(activitiListener.Implementation), CreateFieldDeclarations(activitiListener.FieldExtensions));
        }

        public virtual ITransactionDependentExecutionListener CreateTransactionDependentDelegateExpressionExecutionListener(ActivitiListener activitiListener)
        {
            return new DelegateExpressionTransactionDependentExecutionListener(expressionManager.CreateExpression(activitiListener.Implementation));
        }

        public virtual IActivitiEventListener CreateClassDelegateEventListener(EventListener eventListener)
        {
            return new DelegateActivitiEventListener(eventListener.Implementation, GetEntityType(eventListener.EntityType));
        }

        public virtual IActivitiEventListener CreateDelegateExpressionEventListener(EventListener eventListener)
        {
            return new DelegateExpressionActivitiEventListener(expressionManager.CreateExpression(eventListener.Implementation), GetEntityType(eventListener.EntityType));
        }

        public virtual IActivitiEventListener CreateCustomTaskCompletedEventListener()
        {
            return new CustomTaskCompletedEventListener();
        }

        public virtual IActivitiEventListener CreateEventThrowingEventListener(EventListener eventListener)
        {
            BaseDelegateEventListener result = null;
            if (ImplementationType.IMPLEMENTATION_TYPE_THROW_SIGNAL_EVENT.Equals(eventListener.ImplementationType))
            {
                result = new SignalThrowingEventListener();
                ((SignalThrowingEventListener)result).SignalName = eventListener.Implementation;
                ((SignalThrowingEventListener)result).ProcessInstanceScope = true;
            }
            else if (ImplementationType.IMPLEMENTATION_TYPE_THROW_GLOBAL_SIGNAL_EVENT.Equals(eventListener.ImplementationType))
            {
                result = new SignalThrowingEventListener();
                ((SignalThrowingEventListener)result).SignalName = eventListener.Implementation;
                ((SignalThrowingEventListener)result).ProcessInstanceScope = false;
            }
            else if (ImplementationType.IMPLEMENTATION_TYPE_THROW_MESSAGE_EVENT.Equals(eventListener.ImplementationType))
            {
                result = new MessageThrowingEventListener();
                ((MessageThrowingEventListener)result).MessageName = eventListener.Implementation;
            }
            else if (ImplementationType.IMPLEMENTATION_TYPE_THROW_ERROR_EVENT.Equals(eventListener.ImplementationType))
            {
                result = new ErrorThrowingEventListener();
                ((ErrorThrowingEventListener)result).ErrorCode = eventListener.Implementation;
            }

            if (result == null)
            {
                throw new ActivitiIllegalArgumentException("Cannot create an event-throwing event-listener, unknown implementation type: " + eventListener.ImplementationType);
            }

            result.EntityClass = GetEntityType(eventListener.EntityType);
            return result;
        }

        public virtual ICustomPropertiesResolver CreateClassDelegateCustomPropertiesResolver(ActivitiListener activitiListener)
        {
            return classDelegateFactory.Create(activitiListener.CustomPropertiesResolverImplementation, null);
        }

        public virtual ICustomPropertiesResolver CreateExpressionCustomPropertiesResolver(ActivitiListener activitiListener)
        {
            return new ExpressionCustomPropertiesResolver(expressionManager.CreateExpression(activitiListener.CustomPropertiesResolverImplementation));
        }

        public virtual ICustomPropertiesResolver CreateDelegateExpressionCustomPropertiesResolver(ActivitiListener activitiListener)
        {
            return new DelegateExpressionCustomPropertiesResolver(expressionManager.CreateExpression(activitiListener.CustomPropertiesResolverImplementation));
        }

        /// <param name="entityType">
        ///          the name of the entity
        /// @return </param>
        /// <exception cref="ActivitiIllegalArgumentException">
        ///           when the given entity name </exception>
        protected internal virtual Type GetEntityType(string entityType)
        {
            if (!(entityType is null))
            {
                Type entityClass = ENTITY_MAPPING[entityType.Trim()];
                if (entityClass == null)
                {
                    throw new ActivitiIllegalArgumentException("Unsupported entity-type for an ActivitiEventListener: " + entityType);
                }
                return entityClass;
            }
            return null;
        }
    }

}