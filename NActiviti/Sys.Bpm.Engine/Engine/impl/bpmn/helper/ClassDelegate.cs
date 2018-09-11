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

namespace org.activiti.engine.impl.bpmn.helper
{
    using Newtonsoft.Json.Linq;
    using org.activiti.bpmn.model;
    using org.activiti.engine.@delegate;
    using org.activiti.engine.impl.bpmn.behavior;
    using org.activiti.engine.impl.bpmn.parser;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.@delegate;
    using org.activiti.engine.impl.@delegate.invocation;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;
    using System.Reflection;

    /// <summary>
    /// Helper class for bpmn constructs that allow class delegation.
    /// 
    /// This class will lazily instantiate the referenced classes when needed at runtime.
    /// 
    /// 
    /// 
    /// 
    /// 
    /// </summary>
    [Serializable]
    public class ClassDelegate : AbstractBpmnActivityBehavior, ITaskListener, IExecutionListener, ITransactionDependentExecutionListener, ITransactionDependentTaskListener, @delegate.ISubProcessActivityBehavior, ICustomPropertiesResolver
    {

        private const long serialVersionUID = 1L;

        protected internal string serviceTaskId;
        protected internal string className;
        protected internal IList<FieldDeclaration> fieldDeclarations;
        protected internal IExecutionListener executionListenerInstance;
        protected internal ITransactionDependentExecutionListener transactionDependentExecutionListenerInstance;
        protected internal ITaskListener taskListenerInstance;
        protected internal ITransactionDependentTaskListener transactionDependentTaskListenerInstance;
        protected internal IActivityBehavior activityBehaviorInstance;
        protected internal IExpression skipExpression;
        protected internal IList<MapExceptionEntry> mapExceptions;
        protected internal ICustomPropertiesResolver customPropertiesResolverInstance;

        public ClassDelegate(string className, IList<FieldDeclaration> fieldDeclarations, IExpression skipExpression)
        {
            this.className = className;
            this.fieldDeclarations = fieldDeclarations;
            this.skipExpression = skipExpression;
        }

        public ClassDelegate(string id, string className, IList<FieldDeclaration> fieldDeclarations, IExpression skipExpression, IList<MapExceptionEntry> mapExceptions) : this(className, fieldDeclarations, skipExpression)
        {
            this.serviceTaskId = id;
            this.mapExceptions = mapExceptions;
        }

        public ClassDelegate(string className, IList<FieldDeclaration> fieldDeclarations) : this(className, fieldDeclarations, null)
        {
        }

        public ClassDelegate(Type clazz, IList<FieldDeclaration> fieldDeclarations) : this(clazz.FullName, fieldDeclarations, null)
        {
        }

        public ClassDelegate(Type clazz, IList<FieldDeclaration> fieldDeclarations, IExpression skipExpression) : this(clazz.FullName, fieldDeclarations, skipExpression)
        {
        }

        // Execution listener
        public virtual void notify(IExecutionEntity execution)
        {
            if (executionListenerInstance == null)
            {
                executionListenerInstance = ExecutionListenerInstance;
            }
            Context.ProcessEngineConfiguration.DelegateInterceptor.handleInvocation(new ExecutionListenerInvocation(executionListenerInstance, execution));
        }

        // Transaction Dependent execution listener
        public virtual void notify(string processInstanceId, string executionId, FlowElement flowElement, IDictionary<string, object> executionVariables, IDictionary<string, object> customPropertiesMap)
        {
            if (transactionDependentExecutionListenerInstance == null)
            {
                transactionDependentExecutionListenerInstance = TransactionDependentExecutionListenerInstance;
            }

            // Note that we can't wrap it in the delegate interceptor like usual here due to being executed when the context is already removed.
            transactionDependentExecutionListenerInstance.notify(processInstanceId, executionId, flowElement, executionVariables, customPropertiesMap);
        }

        public virtual IDictionary<string, object> getCustomPropertiesMap(IExecutionEntity execution)
        {
            if (customPropertiesResolverInstance == null)
            {
                customPropertiesResolverInstance = CustomPropertiesResolverInstance;
            }
            return customPropertiesResolverInstance.getCustomPropertiesMap(execution);
        }

        // Task listener
        public virtual void notify(IDelegateTask delegateTask)
        {
            if (taskListenerInstance == null)
            {
                taskListenerInstance = TaskListenerInstance;
            }
            try
            {
                Context.ProcessEngineConfiguration.DelegateInterceptor.handleInvocation(new TaskListenerInvocation(taskListenerInstance, delegateTask));
            }
            catch (Exception e)
            {
                throw new ActivitiException("Exception while invoking TaskListener: " + e.Message, e);
            }
        }

        public virtual void notify(string processInstanceId, string executionId, Task task, IDictionary<string, object> executionVariables, IDictionary<string, object> customPropertiesMap)
        {
            if (transactionDependentTaskListenerInstance == null)
            {
                transactionDependentTaskListenerInstance = TransactionDependentTaskListenerInstance;
            }
            transactionDependentTaskListenerInstance.notify(processInstanceId, executionId, task, executionVariables, customPropertiesMap);
        }


        protected internal virtual IExecutionListener ExecutionListenerInstance
        {
            get
            {
                object delegateInstance = instantiateDelegate(className, fieldDeclarations);
                if (delegateInstance is IExecutionListener)
                {
                    return (IExecutionListener)delegateInstance;
                }
                else if (delegateInstance is IJavaDelegate)
                {
                    return new ServiceTaskJavaDelegateActivityBehavior((IJavaDelegate)delegateInstance);
                }
                else
                {
                    throw new ActivitiIllegalArgumentException(delegateInstance.GetType().FullName + " doesn't implement " + typeof(IExecutionListener) + " nor " + typeof(IJavaDelegate));
                }
            }
        }

        protected internal virtual ITransactionDependentExecutionListener TransactionDependentExecutionListenerInstance
        {
            get
            {
                object delegateInstance = instantiateDelegate(className, fieldDeclarations);
                if (delegateInstance is ITransactionDependentExecutionListener)
                {
                    return (ITransactionDependentExecutionListener)delegateInstance;
                }
                else
                {
                    throw new ActivitiIllegalArgumentException(delegateInstance.GetType().FullName + " doesn't implement " + typeof(ITransactionDependentExecutionListener));
                }
            }
        }

        protected internal virtual ICustomPropertiesResolver CustomPropertiesResolverInstance
        {
            get
            {
                object delegateInstance = instantiateDelegate(className, fieldDeclarations);
                if (delegateInstance is ICustomPropertiesResolver)
                {
                    return (ICustomPropertiesResolver)delegateInstance;
                }
                else
                {
                    throw new ActivitiIllegalArgumentException(delegateInstance.GetType().FullName + " doesn't implement " + typeof(ICustomPropertiesResolver));
                }
            }
        }

        protected internal virtual ITaskListener TaskListenerInstance
        {
            get
            {
                object delegateInstance = instantiateDelegate(className, fieldDeclarations);
                if (delegateInstance is ITaskListener)
                {
                    return (ITaskListener)delegateInstance;
                }
                else
                {
                    throw new ActivitiIllegalArgumentException(delegateInstance.GetType().FullName + " doesn't implement " + typeof(ITaskListener));
                }
            }
        }

        protected internal virtual ITransactionDependentTaskListener TransactionDependentTaskListenerInstance
        {
            get
            {
                object delegateInstance = instantiateDelegate(className, fieldDeclarations);
                if (delegateInstance is ITransactionDependentTaskListener)
                {
                    return (ITransactionDependentTaskListener)delegateInstance;
                }
                else
                {
                    throw new ActivitiIllegalArgumentException(delegateInstance.GetType().FullName + " doesn't implement " + typeof(ITransactionDependentTaskListener));
                }
            }
        }

        // Activity Behavior
        public override void execute(IExecutionEntity execution)
        {
            bool isSkipExpressionEnabled = SkipExpressionUtil.isSkipExpressionEnabled(execution, skipExpression);
            if (!isSkipExpressionEnabled || (isSkipExpressionEnabled && !SkipExpressionUtil.shouldSkipFlowElement(execution, skipExpression)))
            {
                if (Context.ProcessEngineConfiguration.EnableProcessDefinitionInfoCache)
                {
                    JToken taskElementProperties = Context.getBpmnOverrideElementProperties(serviceTaskId, execution.ProcessDefinitionId);
                    if (taskElementProperties != null && taskElementProperties[DynamicBpmnConstants_Fields.SERVICE_TASK_CLASS_NAME] != null)
                    {
                        string overrideClassName = taskElementProperties[DynamicBpmnConstants_Fields.SERVICE_TASK_CLASS_NAME].ToString();
                        if (!string.IsNullOrWhiteSpace(overrideClassName) && !overrideClassName.Equals(className))
                        {
                            className = overrideClassName;
                            activityBehaviorInstance = null;
                        }
                    }
                }

                if (activityBehaviorInstance == null)
                {
                    activityBehaviorInstance = ActivityBehaviorInstance;
                }

                try
                {
                    activityBehaviorInstance.execute(execution);
                }
                catch (BpmnError error)
                {
                    ErrorPropagation.propagateError(error, execution);
                }
                catch (Exception e)
                {
                    if (!ErrorPropagation.mapException(e, execution, mapExceptions))
                    {
                        throw e;
                    }
                }
            }
        }

        // Signallable activity behavior
        public override void trigger(IExecutionEntity execution, string signalName, object signalData)
        {
            if (activityBehaviorInstance == null)
            {
                activityBehaviorInstance = ActivityBehaviorInstance;
            }

            if (activityBehaviorInstance is ITriggerableActivityBehavior)
            {
                ((ITriggerableActivityBehavior)activityBehaviorInstance).trigger(execution, signalName, signalData);
            }
            else
            {
                throw new ActivitiException("signal() can only be called on a " + typeof(ITriggerableActivityBehavior).FullName + " instance");
            }
        }

        // Subprocess activityBehaviour
        public virtual void completing(IExecutionEntity execution, IExecutionEntity subProcessInstance)
        {
            if (activityBehaviorInstance == null)
            {
                activityBehaviorInstance = ActivityBehaviorInstance;
            }

            if (activityBehaviorInstance is @delegate.ISubProcessActivityBehavior)
            {
                ((@delegate.ISubProcessActivityBehavior)activityBehaviorInstance).completing(execution, subProcessInstance);
            }
            else
            {
                throw new ActivitiException("completing() can only be called on a " + typeof(SubProcessActivityBehavior).FullName + " instance");
            }
        }

        public virtual void completed(IExecutionEntity execution)
        {
            if (activityBehaviorInstance == null)
            {
                activityBehaviorInstance = ActivityBehaviorInstance;
            }

            if (activityBehaviorInstance is @delegate.ISubProcessActivityBehavior)
            {
                ((@delegate.ISubProcessActivityBehavior)activityBehaviorInstance).completed(execution);
            }
            else
            {
                throw new ActivitiException("completed() can only be called on a " + activityBehaviorInstance.GetType().FullName + " instance");
            }
        }

        protected internal virtual IActivityBehavior ActivityBehaviorInstance
        {
            get
            {
                object delegateInstance = instantiateDelegate(className, fieldDeclarations);

                if (delegateInstance is IActivityBehavior)
                {
                    return determineBehaviour((IActivityBehavior)delegateInstance);
                }
                else if (delegateInstance is IJavaDelegate)
                {
                    return determineBehaviour(new ServiceTaskJavaDelegateActivityBehavior((IJavaDelegate)delegateInstance));
                }
                else
                {
                    throw new ActivitiIllegalArgumentException(delegateInstance.GetType().FullName + " doesn't implement " + typeof(IJavaDelegate).FullName + " nor " + typeof(IActivityBehavior).FullName);
                }
            }
        }

        // Adds properties to the given delegation instance (eg multi instance) if
        // needed
        protected internal virtual IActivityBehavior determineBehaviour(IActivityBehavior delegateInstance)
        {
            if (hasMultiInstanceCharacteristics())
            {
                multiInstanceActivityBehavior.InnerActivityBehavior = (AbstractBpmnActivityBehavior)delegateInstance;
                return multiInstanceActivityBehavior;
            }
            return delegateInstance;
        }

        protected internal virtual object instantiateDelegate(string className, IList<FieldDeclaration> fieldDeclarations)
        {
            return ClassDelegate.defaultInstantiateDelegate(className, fieldDeclarations);
        }

        // --HELPER METHODS (also usable by external classes)
        // ----------------------------------------

        public static object defaultInstantiateDelegate(Type clazz, IList<FieldDeclaration> fieldDeclarations)
        {
            return defaultInstantiateDelegate(clazz.FullName, fieldDeclarations);
        }

        public static object defaultInstantiateDelegate(string className, IList<FieldDeclaration> fieldDeclarations)
        {
            object @object = ReflectUtil.instantiate(className);
            applyFieldDeclaration(fieldDeclarations, @object);
            return @object;
        }

        public static void applyFieldDeclaration(IList<FieldDeclaration> fieldDeclarations, object target)
        {
            applyFieldDeclaration(fieldDeclarations, target, true);
        }

        public static void applyFieldDeclaration(IList<FieldDeclaration> fieldDeclarations, object target, bool throwExceptionOnMissingField)
        {
            if (fieldDeclarations != null)
            {
                foreach (FieldDeclaration declaration in fieldDeclarations)
                {
                    applyFieldDeclaration(declaration, target, throwExceptionOnMissingField);
                }
            }
        }

        public static void applyFieldDeclaration(FieldDeclaration declaration, object target)
        {
            applyFieldDeclaration(declaration, target, true);
        }

        public static void applyFieldDeclaration(FieldDeclaration declaration, object target, bool throwExceptionOnMissingField)
        {
            MethodInfo setterMethod = ReflectUtil.getSetter(declaration.Name, target.GetType(), declaration.Value.GetType());

            if (setterMethod != null)
            {
                try
                {
                    setterMethod.Invoke(target, new object[] { declaration.Value });
                }
                catch (System.ArgumentException e)
                {
                    throw new ActivitiException("Error while invoking '" + declaration.Name + "' on class " + target.GetType().FullName, e);
                }
                catch (Exception e)
                {
                    throw new ActivitiException("Illegal acces when calling '" + declaration.Name + "' on class " + target.GetType().FullName, e);
                }
                //catch (InvocationTargetException e)
                //{
                //    throw new ActivitiException("Exception while invoking '" + declaration.Name + "' on class " + target.GetType().FullName, e);
                //}
            }
            else
            {
                FieldInfo field = ReflectUtil.getField(declaration.Name, target);
                if (field == null)
                {
                    if (throwExceptionOnMissingField)
                    {
                        throw new ActivitiIllegalArgumentException("Field definition uses unexisting field '" + declaration.Name + "' on class " + target.GetType().FullName);
                    }
                    else
                    {
                        return;
                    }
                }

                // Check if the delegate field's type is correct
                if (!fieldTypeCompatible(declaration, field))
                {
                    throw new ActivitiIllegalArgumentException("Incompatible type set on field declaration '" + declaration.Name + "' for class " + target.GetType().FullName + ". Declared value has type " + declaration.Value.GetType().FullName + ", while expecting " + field.DeclaringType.Name);
                }
                ReflectUtil.setField(field, target, declaration.Value);

            }
        }

        public static bool fieldTypeCompatible(FieldDeclaration declaration, FieldInfo field)
        {
            if (declaration.Value != null)
            {
                return declaration.Value.GetType().IsAssignableFrom(field.DeclaringType);
            }
            else
            {
                // Null can be set any field type
                return true;
            }
        }

        /// <summary>
        /// returns the class name this <seealso cref="ClassDelegate"/> is configured to. Comes in handy if you want to check which delegates you already have e.g. in a list of listeners
        /// </summary>
        public virtual string ClassName
        {
            get
            {
                return className;
            }
        }

    }

}