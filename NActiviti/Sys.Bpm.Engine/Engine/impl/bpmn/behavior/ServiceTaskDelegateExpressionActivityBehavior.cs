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
namespace Sys.Workflow.Engine.Impl.Bpmn.Behavior
{
    using Newtonsoft.Json.Linq;
    using Sys.Workflow.Engine.Delegate;
    using Sys.Workflow.Engine.Impl.Bpmn.Helper;
    using Sys.Workflow.Engine.Impl.Bpmn.Parser;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Delegate;
    using Sys.Workflow.Engine.Impl.Delegate.Invocation;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;

    /// <summary>
    /// <seealso cref="IActivityBehavior"/> used when 'delegateExpression' is used for a serviceTask.
    /// 
    /// </summary>
    [Serializable]
    public class ServiceTaskDelegateExpressionActivityBehavior : TaskActivityBehavior
    {

        private const long serialVersionUID = 1L;

        protected internal string serviceTaskId;
        protected internal IExpression expression;
        protected internal IExpression skipExpression;
        private readonly IList<FieldDeclaration> fieldDeclarations;

        public ServiceTaskDelegateExpressionActivityBehavior(string serviceTaskId, IExpression expression, IExpression skipExpression, IList<FieldDeclaration> fieldDeclarations)
        {
            this.serviceTaskId = serviceTaskId;
            this.expression = expression;
            this.skipExpression = skipExpression;
            this.fieldDeclarations = fieldDeclarations;
        }

        public override void Trigger(IExecutionEntity execution, string signalName, object signalData, bool throwError = true)
        {
            object @delegate = DelegateExpressionUtil.ResolveDelegateExpression(expression, execution, fieldDeclarations);
            if (@delegate is ITriggerableActivityBehavior)
            {
                ((ITriggerableActivityBehavior)@delegate).Trigger(execution, signalName, signalData);
            }
        }

        public override void Execute(IExecutionEntity execution)
        {

            try
            {
                bool isSkipExpressionEnabled = SkipExpressionUtil.IsSkipExpressionEnabled(execution, skipExpression);
                if (!isSkipExpressionEnabled || (isSkipExpressionEnabled && !SkipExpressionUtil.ShouldSkipFlowElement(execution, skipExpression)))
                {

                    if (Context.ProcessEngineConfiguration.EnableProcessDefinitionInfoCache)
                    {
                        JToken taskElementProperties = Context.GetBpmnOverrideElementProperties(serviceTaskId, execution.ProcessDefinitionId);
                        if (taskElementProperties is object && taskElementProperties[DynamicBpmnConstants.SERVICE_TASK_DELEGATE_EXPRESSION] is object)
                        {
                            string overrideExpression = taskElementProperties[DynamicBpmnConstants.SERVICE_TASK_DELEGATE_EXPRESSION].ToString();
                            if (!string.IsNullOrWhiteSpace(overrideExpression) && !overrideExpression.Equals(expression.ExpressionText))
                            {
                                expression = Context.ProcessEngineConfiguration.ExpressionManager.CreateExpression(overrideExpression);
                            }
                        }
                    }

                    object @delegate = DelegateExpressionUtil.ResolveDelegateExpression(expression, execution, fieldDeclarations);
                    if (@delegate is IActivityBehavior)
                    {

                        if (@delegate is AbstractBpmnActivityBehavior)
                        {
                            ((AbstractBpmnActivityBehavior)@delegate).MultiInstanceActivityBehavior = MultiInstanceActivityBehavior;
                        }

                        Context.ProcessEngineConfiguration.DelegateInterceptor.HandleInvocation(new ActivityBehaviorInvocation((IActivityBehavior)@delegate, execution));

                    }
                    else if (@delegate is ICSharpDelegate)
                    {
                        Context.ProcessEngineConfiguration.DelegateInterceptor.HandleInvocation(new CSharpDelegateInvocation((ICSharpDelegate)@delegate, execution));
                        Leave(execution);

                    }
                    else
                    {
                        throw new ActivitiIllegalArgumentException("Delegate expression " + expression + " did neither resolve to an implementation of " + typeof(IActivityBehavior) + " nor " + typeof(ICSharpDelegate));
                    }
                }
                else
                {
                    Leave(execution);
                }
            }
            catch (Exception exc)
            {

                Exception cause = exc;
                BpmnError error = null;
                while (cause is object)
                {
                    if (cause is BpmnError)
                    {
                        error = (BpmnError)cause;
                        break;
                    }
                    cause = cause.InnerException;
                }

                if (error is object)
                {
                    ErrorPropagation.PropagateError(error, execution);
                }
                else
                {
                    throw new ActivitiException(exc.Message, exc);
                }

            }
        }

    }

}