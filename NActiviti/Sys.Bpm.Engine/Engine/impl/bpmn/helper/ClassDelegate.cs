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

namespace Sys.Workflow.Engine.Impl.Bpmn.Helper
{
    using Newtonsoft.Json.Linq;
    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine.Delegate;
    using Sys.Workflow.Engine.Impl.Bpmn.Behavior;
    using Sys.Workflow.Engine.Impl.Bpmn.Parser;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Delegate;
    using Sys.Workflow.Engine.Impl.Delegate.Invocation;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Impl.Util;
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
    public class ClassDelegate : AbstractBpmnActivityBehavior, ITaskListener, IExecutionListener, ITransactionDependentExecutionListener, ITransactionDependentTaskListener, ISubProcessActivityBehavior, ICustomPropertiesResolver
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
        public virtual void Notify(IExecutionEntity execution)
        {
            if (executionListenerInstance is null)
            {
                executionListenerInstance = ExecutionListenerInstance;
            }
            Context.ProcessEngineConfiguration.DelegateInterceptor.HandleInvocation(new ExecutionListenerInvocation(executionListenerInstance, execution));
        }

        // Transaction Dependent execution listener
        public virtual void Notify(string processInstanceId, string executionId, FlowElement flowElement, IDictionary<string, object> executionVariables, IDictionary<string, object> customPropertiesMap)
        {
            if (transactionDependentExecutionListenerInstance is null)
            {
                transactionDependentExecutionListenerInstance = TransactionDependentExecutionListenerInstance;
            }

            // Note that we can't wrap it in the delegate interceptor like usual here due to being executed when the context is already removed.
            transactionDependentExecutionListenerInstance.Notify(processInstanceId, executionId, flowElement, executionVariables, customPropertiesMap);
        }

        public virtual IDictionary<string, object> GetCustomPropertiesMap(IExecutionEntity execution)
        {
            if (customPropertiesResolverInstance is null)
            {
                customPropertiesResolverInstance = CustomPropertiesResolverInstance;
            }
            return customPropertiesResolverInstance.GetCustomPropertiesMap(execution);
        }

        // Task listener
        public virtual void Notify(IDelegateTask delegateTask)
        {
            if (taskListenerInstance is null)
            {
                taskListenerInstance = TaskListenerInstance;
            }
            try
            {
                Context.ProcessEngineConfiguration.DelegateInterceptor.HandleInvocation(new TaskListenerInvocation(taskListenerInstance, delegateTask));
            }
            catch (Exception e)
            {
                throw new ActivitiException("Exception while invoking TaskListener: " + e.Message, e);
            }
        }

        public virtual void Notify(string processInstanceId, string executionId, TaskActivity task, IDictionary<string, object> executionVariables, IDictionary<string, object> customPropertiesMap)
        {
            if (transactionDependentTaskListenerInstance is null)
            {
                transactionDependentTaskListenerInstance = TransactionDependentTaskListenerInstance;
            }
            transactionDependentTaskListenerInstance.Notify(processInstanceId, executionId, task, executionVariables, customPropertiesMap);
        }


        protected internal virtual IExecutionListener ExecutionListenerInstance
        {
            get
            {
                object delegateInstance = InstantiateDelegate(className, fieldDeclarations);
                if (delegateInstance is IExecutionListener)
                {
                    return (IExecutionListener)delegateInstance;
                }
                else if (delegateInstance is ICSharpDelegate)
                {
                    return new ServiceTaskCSharpDelegateActivityBehavior((ICSharpDelegate)delegateInstance);
                }
                else
                {
                    throw new ActivitiIllegalArgumentException(delegateInstance.GetType().FullName + " doesn't implement " + typeof(IExecutionListener) + " nor " + typeof(ICSharpDelegate));
                }
            }
        }

        protected internal virtual ITransactionDependentExecutionListener TransactionDependentExecutionListenerInstance
        {
            get
            {
                object delegateInstance = InstantiateDelegate(className, fieldDeclarations);
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
                object delegateInstance = InstantiateDelegate(className, fieldDeclarations);
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
                object delegateInstance = InstantiateDelegate(className, fieldDeclarations);
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
                object delegateInstance = InstantiateDelegate(className, fieldDeclarations);
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
        public override void Execute(IExecutionEntity execution)
        {
            bool isSkipExpressionEnabled = SkipExpressionUtil.IsSkipExpressionEnabled(execution, skipExpression);
            if (!isSkipExpressionEnabled || (isSkipExpressionEnabled && !SkipExpressionUtil.ShouldSkipFlowElement(execution, skipExpression)))
            {
                if (Context.ProcessEngineConfiguration.EnableProcessDefinitionInfoCache)
                {
                    JToken taskElementProperties = Context.GetBpmnOverrideElementProperties(serviceTaskId, execution.ProcessDefinitionId);
                    if (taskElementProperties is object && taskElementProperties[DynamicBpmnConstants.SERVICE_TASK_CLASS_NAME] is object)
                    {
                        string overrideClassName = taskElementProperties[DynamicBpmnConstants.SERVICE_TASK_CLASS_NAME].ToString();
                        if (!string.IsNullOrWhiteSpace(overrideClassName) && !overrideClassName.Equals(className))
                        {
                            className = overrideClassName;
                            activityBehaviorInstance = null;
                        }
                    }
                }

                if (activityBehaviorInstance is null)
                {
                    activityBehaviorInstance = ActivityBehaviorInstance;
                }

                try
                {
                    activityBehaviorInstance.Execute(execution);
                }
                catch (BpmnError error)
                {
                    ErrorPropagation.PropagateError(error, execution);
                }
                catch (Exception e)
                {
                    if (!ErrorPropagation.MapException(e, execution, mapExceptions))
                    {
                        throw;
                    }
                }
            }
        }

        // Signallable activity behavior
        public override void Trigger(IExecutionEntity execution, string signalName, object signalData, bool throwError = true)
        {
            if (activityBehaviorInstance is null)
            {
                activityBehaviorInstance = ActivityBehaviorInstance;
            }

            if (activityBehaviorInstance is ITriggerableActivityBehavior)
            {
                ((ITriggerableActivityBehavior)activityBehaviorInstance).Trigger(execution, signalName, signalData);
            }
            else
            {
                throw new ActivitiException("signal() can only be called on a " + typeof(ITriggerableActivityBehavior).FullName + " instance");
            }
        }

        // Subprocess activityBehaviour
        public virtual void Completing(IExecutionEntity execution, IExecutionEntity subProcessInstance)
        {
            if (activityBehaviorInstance is null)
            {
                activityBehaviorInstance = ActivityBehaviorInstance;
            }

            if (activityBehaviorInstance is ISubProcessActivityBehavior)
            {
                ((ISubProcessActivityBehavior)activityBehaviorInstance).Completing(execution, subProcessInstance);
            }
            else
            {
                throw new ActivitiException("completing() can only be called on a " + typeof(SubProcessActivityBehavior).FullName + " instance");
            }
        }

        public virtual void Completed(IExecutionEntity execution)
        {
            if (activityBehaviorInstance is null)
            {
                activityBehaviorInstance = ActivityBehaviorInstance;
            }

            if (activityBehaviorInstance is ISubProcessActivityBehavior)
            {
                ((ISubProcessActivityBehavior)activityBehaviorInstance).Completed(execution);
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
                object delegateInstance = InstantiateDelegate(className, fieldDeclarations);

                if (delegateInstance is IActivityBehavior)
                {
                    return DetermineBehaviour((IActivityBehavior)delegateInstance);
                }
                else if (delegateInstance is ICSharpDelegate)
                {
                    return DetermineBehaviour(new ServiceTaskCSharpDelegateActivityBehavior((ICSharpDelegate)delegateInstance));
                }
                else
                {
                    throw new ActivitiIllegalArgumentException(delegateInstance.GetType().FullName + " doesn't implement " + typeof(ICSharpDelegate).FullName + " nor " + typeof(IActivityBehavior).FullName);
                }
            }
        }

        // Adds properties to the given delegation instance (eg multi instance) if
        // needed
        protected internal virtual IActivityBehavior DetermineBehaviour(IActivityBehavior delegateInstance)
        {
            if (HasMultiInstanceCharacteristics())
            {
                multiInstanceActivityBehavior.InnerActivityBehavior = (AbstractBpmnActivityBehavior)delegateInstance;
                return multiInstanceActivityBehavior;
            }
            return delegateInstance;
        }

        protected internal virtual object InstantiateDelegate(string className, IList<FieldDeclaration> fieldDeclarations)
        {
            return DefaultInstantiateDelegate(className, fieldDeclarations);
        }

        // --HELPER METHODS (also usable by external classes)
        // ----------------------------------------

        public static object DefaultInstantiateDelegate(Type clazz, IList<FieldDeclaration> fieldDeclarations)
        {
            return DefaultInstantiateDelegate(clazz.FullName, fieldDeclarations);
        }

        public static object DefaultInstantiateDelegate(string className, IList<FieldDeclaration> fieldDeclarations)
        {
            object @object = ReflectUtil.Instantiate(className);
            ApplyFieldDeclaration(fieldDeclarations, @object);
            return @object;
        }

        public static void ApplyFieldDeclaration(IList<FieldDeclaration> fieldDeclarations, object target)
        {
            ApplyFieldDeclaration(fieldDeclarations, target, true);
        }

        public static void ApplyFieldDeclaration(IList<FieldDeclaration> fieldDeclarations, object target, bool throwExceptionOnMissingField)
        {
            if (fieldDeclarations is object)
            {
                foreach (FieldDeclaration declaration in fieldDeclarations)
                {
                    ApplyFieldDeclaration(declaration, target, throwExceptionOnMissingField);
                }
            }
        }

        public static void ApplyFieldDeclaration(FieldDeclaration declaration, object target)
        {
            ApplyFieldDeclaration(declaration, target, true);
        }

        public static void ApplyFieldDeclaration(FieldDeclaration declaration, object target, bool throwExceptionOnMissingField)
        {
            MethodInfo setterMethod = ReflectUtil.GetSetter(declaration.Name, target.GetType(), declaration.Value.GetType());

            if (setterMethod is object)
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
                FieldInfo field = ReflectUtil.GetField(declaration.Name, target);
                if (field is null)
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
                if (!FieldTypeCompatible(declaration, field))
                {
                    throw new ActivitiIllegalArgumentException("Incompatible type set on field declaration '" + declaration.Name + "' for class " + target.GetType().FullName + ". Declared value has type " + declaration.Value.GetType().FullName + ", while expecting " + field.DeclaringType.Name);
                }
                ReflectUtil.SetField(field, target, declaration.Value);

            }
        }

        public static bool FieldTypeCompatible(FieldDeclaration declaration, FieldInfo field)
        {
            if (declaration.Value is object)
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