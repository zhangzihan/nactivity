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
namespace Sys.Workflow.Engine.Impl.Bpmn.Parser.Factory
{
    using Sys.Workflow.Bpmn.Constants;
    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine.Delegate;
    using Sys.Workflow.Engine.Impl.Bpmn.Behavior;
    using Sys.Workflow.Engine.Impl.Bpmn.Helper;
    using Sys.Workflow.Engine.Impl.Delegate;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Default implementation of the <seealso cref="IActivityBehaviorFactory"/>. Used when no custom <seealso cref="IActivityBehaviorFactory"/> is injected on the <seealso cref="ProcessEngineConfigurationImpl"/>.
    /// </summary>
    public class DefaultActivityBehaviorFactory : AbstractBehaviorFactory, IActivityBehaviorFactory
    {

        public const string DEFAULT_SERVICE_TASK_BEAN_NAME = "defaultServiceTaskBehavior";
        private readonly IClassDelegateFactory classDelegateFactory;
        private static readonly Regex EXPR_PATTERN = new Regex("\\$+\\{+.+\\}");

        public DefaultActivityBehaviorFactory(IClassDelegateFactory classDelegateFactory)
        {
            this.classDelegateFactory = classDelegateFactory;
        }

        public DefaultActivityBehaviorFactory() : this(new DefaultClassDelegateFactory())
        {
        }

        // Start event
        public const string EXCEPTION_MAP_FIELD = "mapExceptions";

        public virtual NoneStartEventActivityBehavior CreateNoneStartEventActivityBehavior(StartEvent startEvent)
        {
            return new NoneStartEventActivityBehavior();
        }

        // Task

        public virtual TaskActivityBehavior CreateTaskActivityBehavior(TaskActivity task)
        {
            return new TaskActivityBehavior();
        }

        public virtual ManualTaskActivityBehavior CreateManualTaskActivityBehavior(ManualTask manualTask)
        {
            return new ManualTaskActivityBehavior();
        }

        public virtual ReceiveTaskActivityBehavior CreateReceiveTaskActivityBehavior(ReceiveTask receiveTask)
        {
            return new ReceiveTaskActivityBehavior();
        }

        public virtual UserTaskActivityBehavior CreateUserTaskActivityBehavior(UserTask userTask)
        {
            return new UserTaskActivityBehavior(userTask);
        }

        // Service task

        protected internal virtual IExpression GetSkipExpressionFromServiceTask(ServiceTask serviceTask)
        {
            IExpression result = null;
            if (!string.IsNullOrWhiteSpace(serviceTask.SkipExpression))
            {
                result = expressionManager.CreateExpression(serviceTask.SkipExpression);
            }
            return result;
        }

        public virtual ClassDelegate CreateClassDelegateSendTask(SendTask sendTask)
        {
            return classDelegateFactory.Create(sendTask.Id, sendTask.Implementation, CreateFieldDeclarations(sendTask.FieldExtensions), null, sendTask.MapExceptions);
        }

        public virtual ClassDelegate CreateClassDelegateServiceTask(ServiceTask serviceTask)
        {
            return classDelegateFactory.Create(serviceTask.Id, serviceTask.Implementation, CreateFieldDeclarations(serviceTask.FieldExtensions), GetSkipExpressionFromServiceTask(serviceTask), serviceTask.MapExceptions);
        }

        public virtual ServiceTaskDelegateExpressionActivityBehavior CreateServiceTaskDelegateExpressionActivityBehavior(ServiceTask serviceTask)
        {
            IExpression delegateExpression = expressionManager.CreateExpression(serviceTask.Implementation);
            return CreateServiceTaskBehavior(serviceTask, delegateExpression);
        }

        public virtual IActivityBehavior CreateDefaultServiceTaskBehavior(ServiceTask serviceTask)
        {
            // this is covering the case where only the field `implementation` was defined in the process definition. I.e.
            // <serviceTask id="serviceTask" implementation="myServiceTaskImpl"/>
            // `myServiceTaskImpl` can be different things depending on the implementation of `defaultServiceTaskBehavior`
            // could be for instance a Spring bean or a target for a Spring Stream
            IExpression delegateExpression = expressionManager.CreateExpression("${" + DEFAULT_SERVICE_TASK_BEAN_NAME + "}");
            return CreateServiceTaskBehavior(serviceTask, delegateExpression);
        }

        private ServiceTaskDelegateExpressionActivityBehavior CreateServiceTaskBehavior(ServiceTask serviceTask, IExpression delegateExpression)
        {
            return new ServiceTaskDelegateExpressionActivityBehavior(serviceTask.Id, delegateExpression, GetSkipExpressionFromServiceTask(serviceTask), CreateFieldDeclarations(serviceTask.FieldExtensions));
        }

        public virtual ServiceTaskExpressionActivityBehavior CreateServiceTaskExpressionActivityBehavior(ServiceTask serviceTask)
        {
            IExpression expression = expressionManager.CreateExpression(serviceTask.Implementation);
            return new ServiceTaskExpressionActivityBehavior(serviceTask.Id, expression, GetSkipExpressionFromServiceTask(serviceTask), serviceTask.ResultVariableName);
        }

        public virtual WebServiceActivityBehavior CreateWebServiceActivityBehavior(ServiceTask serviceTask)
        {
            return new WebServiceActivityBehavior();
        }

        public virtual WebServiceActivityBehavior CreateWebServiceActivityBehavior(SendTask sendTask)
        {
            return new WebServiceActivityBehavior();
        }

        public virtual MailActivityBehavior CreateMailActivityBehavior(ServiceTask serviceTask)
        {
            return CreateMailActivityBehavior(serviceTask.Id, serviceTask.FieldExtensions);
        }

        public virtual MailActivityBehavior CreateMailActivityBehavior(SendTask sendTask)
        {
            return CreateMailActivityBehavior(sendTask.Id, sendTask.FieldExtensions);
        }

        protected internal virtual MailActivityBehavior CreateMailActivityBehavior(string taskId, IList<FieldExtension> fields)
        {
            IList<FieldDeclaration> fieldDeclarations = CreateFieldDeclarations(fields);
            return (MailActivityBehavior)ClassDelegate.DefaultInstantiateDelegate(typeof(MailActivityBehavior), fieldDeclarations);
        }

        // We do not want a hard dependency on Mule, hence we return
        // ActivityBehavior and instantiate the delegate instance using a string instead of the Class itself.
        public virtual IActivityBehavior CreateMuleActivityBehavior(ServiceTask serviceTask)
        {
            return CreateMuleActivityBehavior(serviceTask, serviceTask.FieldExtensions);
        }

        public virtual IActivityBehavior CreateMuleActivityBehavior(SendTask sendTask)
        {
            return CreateMuleActivityBehavior(sendTask, sendTask.FieldExtensions);
        }

        protected internal virtual IActivityBehavior CreateMuleActivityBehavior(TaskWithFieldExtensions task, IList<FieldExtension> fieldExtensions)
        {
            try
            {

                Type theClass = Type.GetType("Sys.Workflow.Mule.MuleSendActivitiBehavior");
                IList<FieldDeclaration> fieldDeclarations = CreateFieldDeclarations(fieldExtensions);
                return (IActivityBehavior)ClassDelegate.DefaultInstantiateDelegate(theClass, fieldDeclarations);
            }
            catch (Exception e)
            {
                throw new ActivitiException("Could not find Sys.Workflow.Mule.MuleSendActivitiBehavior: ", e);
            }
        }

        // We do not want a hard dependency on Camel, hence we return
        // ActivityBehavior and instantiate the delegate instance using a string instead of the Class itself.
        public virtual IActivityBehavior CreateCamelActivityBehavior(ServiceTask serviceTask)
        {
            return CreateCamelActivityBehavior(serviceTask, serviceTask.FieldExtensions);
        }

        public virtual IActivityBehavior CreateCamelActivityBehavior(SendTask sendTask)
        {
            return CreateCamelActivityBehavior(sendTask, sendTask.FieldExtensions);
        }

        protected internal virtual IActivityBehavior CreateCamelActivityBehavior(TaskWithFieldExtensions task, IList<FieldExtension> fieldExtensions)
        {
            try
            {
                Type theClass = null;
                FieldExtension behaviorExtension = null;
                foreach (FieldExtension fieldExtension in fieldExtensions)
                {
                    if ("camelBehaviorClass".Equals(fieldExtension.FieldName) && !string.IsNullOrWhiteSpace(fieldExtension.StringValue))
                    {
                        theClass = Type.GetType(fieldExtension.StringValue);
                        behaviorExtension = fieldExtension;
                        break;
                    }
                }

                if (behaviorExtension != null)
                {
                    fieldExtensions.Remove(behaviorExtension);
                }

                if (theClass == null)
                {
                    // Default Camel behavior class
                    theClass = Type.GetType("Sys.Workflow.Camel.Impl.CamelBehaviorDefaultImpl");
                }

                IList<FieldDeclaration> fieldDeclarations = CreateFieldDeclarations(fieldExtensions);
                AddExceptionMapAsFieldDeclaration(fieldDeclarations, task.MapExceptions);
                return (IActivityBehavior)ClassDelegate.DefaultInstantiateDelegate(theClass, fieldDeclarations);
            }
            catch (Exception e)
            {
                throw new ActivitiException("Could not find Sys.Workflow.Camel.CamelBehavior: ", e);
            }
        }

        private void AddExceptionMapAsFieldDeclaration(IList<FieldDeclaration> fieldDeclarations, IList<MapExceptionEntry> mapExceptions)
        {
            FieldDeclaration exceptionMapsFieldDeclaration = new FieldDeclaration(EXCEPTION_MAP_FIELD, mapExceptions.GetType().ToString(), mapExceptions);
            fieldDeclarations.Add(exceptionMapsFieldDeclaration);
        }

        public virtual ShellActivityBehavior CreateShellActivityBehavior(ServiceTask serviceTask)
        {
            IList<FieldDeclaration> fieldDeclarations = CreateFieldDeclarations(serviceTask.FieldExtensions);
            return (ShellActivityBehavior)ClassDelegate.DefaultInstantiateDelegate(typeof(ShellActivityBehavior), fieldDeclarations);
        }

        public virtual IActivityBehavior CreateBusinessRuleTaskActivityBehavior(BusinessRuleTask businessRuleTask)
        {
            IBusinessRuleTaskDelegate ruleActivity = null;
            if (!string.IsNullOrWhiteSpace(businessRuleTask.ClassName))
            {
                try
                {
                    Type clazz = Type.GetType(businessRuleTask.ClassName);
                    ruleActivity = (IBusinessRuleTaskDelegate)Activator.CreateInstance(clazz);
                }
                catch (Exception e)
                {
                    throw new ActivitiException("Could not instantiate businessRuleTask (id:" + businessRuleTask.Id + ") class: " + businessRuleTask.ClassName, e);
                }
            }
            else
            {
                // no default behavior
            }

            foreach (string ruleVariableInputObject in businessRuleTask.InputVariables)
            {
                ruleActivity.AddRuleVariableInputIdExpression(expressionManager.CreateExpression(ruleVariableInputObject.Trim()));
            }

            foreach (string rule in businessRuleTask.RuleNames)
            {
                ruleActivity.AddRuleIdExpression(expressionManager.CreateExpression(rule.Trim()));
            }

            ruleActivity.Exclude = businessRuleTask.Exclude;

            if (businessRuleTask.ResultVariableName is object && businessRuleTask.ResultVariableName.Length > 0)
            {
                ruleActivity.ResultVariable = businessRuleTask.ResultVariableName;
            }
            else
            {
                ruleActivity.ResultVariable = "Sys.Workflow.Engine.rules.OUTPUT";
            }

            return ruleActivity;
        }

        // Script task

        public virtual ScriptTaskActivityBehavior CreateScriptTaskActivityBehavior(ScriptTask scriptTask)
        {
            string language = scriptTask.ScriptFormat;
            if (language is null)
            {
                //language = ScriptingEngines.DEFAULT_SCRIPTING_LANGUAGE;
            }
            return new ScriptTaskActivityBehavior(scriptTask.Id, scriptTask.Script, language, scriptTask.ResultVariable, scriptTask.AutoStoreVariables);
        }

        // Gateways

        public virtual ExclusiveGatewayActivityBehavior CreateExclusiveGatewayActivityBehavior(ExclusiveGateway exclusiveGateway)
        {
            return new ExclusiveGatewayActivityBehavior();
        }

        public virtual ParallelGatewayActivityBehavior CreateParallelGatewayActivityBehavior(ParallelGateway parallelGateway)
        {
            return new ParallelGatewayActivityBehavior();
        }

        public virtual InclusiveGatewayActivityBehavior CreateInclusiveGatewayActivityBehavior(InclusiveGateway inclusiveGateway)
        {
            return new InclusiveGatewayActivityBehavior();
        }

        public virtual EventBasedGatewayActivityBehavior CreateEventBasedGatewayActivityBehavior(EventGateway eventGateway)
        {
            return new EventBasedGatewayActivityBehavior();
        }

        // Multi Instance

        public virtual SequentialMultiInstanceBehavior CreateSequentialMultiInstanceBehavior(Activity activity, AbstractBpmnActivityBehavior innerActivityBehavior)
        {
            return new SequentialMultiInstanceBehavior(activity, innerActivityBehavior);
        }

        public virtual ParallelMultiInstanceBehavior CreateParallelMultiInstanceBehavior(Activity activity, AbstractBpmnActivityBehavior innerActivityBehavior)
        {
            ExtensionAttribute assigneeType = activity.AssigneeType;

            switch (assigneeType?.Value?.ToLower())
            {
                case AssigneeType.SINGLE:
                case AssigneeType.CUSTOM:
                default:
                    return new ParallelMultiInstanceBehavior(activity, innerActivityBehavior);
                case AssigneeType.SINGLE_PASS:
                    return new SinglePassParallelMultiInstanceBehavior(activity, innerActivityBehavior);
                case AssigneeType.ONE:
                    return new OnePassParallelMultiInstanceBehavior(activity, innerActivityBehavior);
                case AssigneeType.HALF_PASSED:
                case AssigneeType.HALF_REJECT:
                    return new HalfPassParallelMultiInstanceBehavior(activity, innerActivityBehavior);
                case AssigneeType.ALL:
                    return new AllPassParallelMultiInstanceBehavior(activity, innerActivityBehavior);
            }
        }

        // Subprocess

        public virtual SubProcessActivityBehavior CreateSubprocessActivityBehavior(SubProcess subProcess)
        {
            return new SubProcessActivityBehavior();
        }

        public virtual EventSubProcessErrorStartEventActivityBehavior CreateEventSubProcessErrorStartEventActivityBehavior(StartEvent startEvent)
        {
            return new EventSubProcessErrorStartEventActivityBehavior();
        }

        public virtual EventSubProcessMessageStartEventActivityBehavior CreateEventSubProcessMessageStartEventActivityBehavior(StartEvent startEvent, MessageEventDefinition messageEventDefinition)
        {
            return new EventSubProcessMessageStartEventActivityBehavior(messageEventDefinition);
        }

        public virtual AdhocSubProcessActivityBehavior CreateAdhocSubprocessActivityBehavior(SubProcess subProcess)
        {
            return new AdhocSubProcessActivityBehavior();
        }

        // Call activity

        public virtual CallActivityBehavior CreateCallActivityBehavior(CallActivity callActivity)
        {
            CallActivityBehavior callActivityBehaviour;
            if (!string.IsNullOrWhiteSpace(callActivity.CalledElement) && EXPR_PATTERN.IsMatch(callActivity.CalledElement))
            {
                callActivityBehaviour = new CallActivityBehavior(expressionManager.CreateExpression(callActivity.CalledElement), callActivity.MapExceptions);
            }
            else
            {
                callActivityBehaviour = new CallActivityBehavior(callActivity.CalledElement, callActivity.MapExceptions);
            }

            return callActivityBehaviour;
        }

        // Transaction

        public virtual TransactionActivityBehavior CreateTransactionActivityBehavior(Transaction transaction)
        {
            return new TransactionActivityBehavior();
        }

        // Intermediate Events

        public virtual IntermediateCatchEventActivityBehavior CreateIntermediateCatchEventActivityBehavior(IntermediateCatchEvent intermediateCatchEvent)
        {
            return new IntermediateCatchEventActivityBehavior();
        }

        public virtual IntermediateCatchMessageEventActivityBehavior CreateIntermediateCatchMessageEventActivityBehavior(IntermediateCatchEvent intermediateCatchEvent, MessageEventDefinition messageEventDefinition)
        {
            return new IntermediateCatchMessageEventActivityBehavior(messageEventDefinition);
        }

        public virtual IntermediateCatchTimerEventActivityBehavior CreateIntermediateCatchTimerEventActivityBehavior(IntermediateCatchEvent intermediateCatchEvent, TimerEventDefinition timerEventDefinition)
        {
            return new IntermediateCatchTimerEventActivityBehavior(timerEventDefinition);
        }

        public virtual IntermediateCatchSignalEventActivityBehavior CreateIntermediateCatchSignalEventActivityBehavior(IntermediateCatchEvent intermediateCatchEvent, SignalEventDefinition signalEventDefinition, Signal signal)
        {

            return new IntermediateCatchSignalEventActivityBehavior(signalEventDefinition, signal);
        }

        public virtual IntermediateThrowNoneEventActivityBehavior CreateIntermediateThrowNoneEventActivityBehavior(ThrowEvent throwEvent)
        {
            return new IntermediateThrowNoneEventActivityBehavior();
        }

        public virtual IntermediateThrowMessageEventActivityBehavior CreateIntermediateThrowMessgeEventActivityBehavior(ThrowEvent throwEvent, MessageEventDefinition messageEventDefinition, Message message)
        {
            return new IntermediateThrowMessageEventActivityBehavior(messageEventDefinition, message);
        }

        public virtual IntermediateThrowSignalEventActivityBehavior CreateIntermediateThrowSignalEventActivityBehavior(ThrowEvent throwEvent, SignalEventDefinition signalEventDefinition, Signal signal)
        {

            return new IntermediateThrowSignalEventActivityBehavior(signalEventDefinition, signal);
        }

        public virtual IntermediateThrowCompensationEventActivityBehavior CreateIntermediateThrowCompensationEventActivityBehavior(ThrowEvent throwEvent, CompensateEventDefinition compensateEventDefinition)
        {
            return new IntermediateThrowCompensationEventActivityBehavior(compensateEventDefinition);
        }

        // End events

        public virtual NoneEndEventActivityBehavior CreateNoneEndEventActivityBehavior(EndEvent endEvent)
        {
            return new NoneEndEventActivityBehavior();
        }

        public virtual ErrorEndEventActivityBehavior CreateErrorEndEventActivityBehavior(EndEvent endEvent, ErrorEventDefinition errorEventDefinition)
        {
            return new ErrorEndEventActivityBehavior(errorEventDefinition.ErrorCode);
        }

        public virtual CancelEndEventActivityBehavior CreateCancelEndEventActivityBehavior(EndEvent endEvent)
        {
            return new CancelEndEventActivityBehavior();
        }

        public virtual TerminateEndEventActivityBehavior CreateTerminateEndEventActivityBehavior(EndEvent endEvent)
        {
            bool terminateAll = false;
            bool terminateMultiInstance = false;

            if (endEvent.EventDefinitions != null && endEvent.EventDefinitions.Count > 0 && endEvent.EventDefinitions[0] is TerminateEventDefinition)
            {
                terminateAll = ((TerminateEventDefinition)endEvent.EventDefinitions[0]).TerminateAll;
                terminateMultiInstance = ((TerminateEventDefinition)endEvent.EventDefinitions[0]).TerminateMultiInstance;
            }

            var extTerminateAll = endEvent.GetExtensionElementAttributeValue(nameof(terminateAll));
            bool.TryParse(extTerminateAll, out terminateAll);

            var extTerminateMul = endEvent.GetExtensionElementAttributeValue(nameof(terminateMultiInstance));
            bool.TryParse(extTerminateMul, out terminateMultiInstance);

            TerminateEndEventActivityBehavior terminateEndEventActivityBehavior = new TerminateEndEventActivityBehavior
            {
                TerminateAll = terminateAll,
                TerminateMultiInstance = terminateMultiInstance
            };
            return terminateEndEventActivityBehavior;
        }

        // Boundary Events

        public virtual BoundaryEventActivityBehavior CreateBoundaryEventActivityBehavior(BoundaryEvent boundaryEvent, bool interrupting)
        {
            return new BoundaryEventActivityBehavior(interrupting);
        }

        public virtual BoundaryCancelEventActivityBehavior CreateBoundaryCancelEventActivityBehavior(CancelEventDefinition cancelEventDefinition)
        {
            return new BoundaryCancelEventActivityBehavior();
        }

        public virtual BoundaryCompensateEventActivityBehavior CreateBoundaryCompensateEventActivityBehavior(BoundaryEvent boundaryEvent, CompensateEventDefinition compensateEventDefinition, bool interrupting)
        {

            return new BoundaryCompensateEventActivityBehavior(compensateEventDefinition, interrupting);
        }

        public virtual BoundaryTimerEventActivityBehavior CreateBoundaryTimerEventActivityBehavior(BoundaryEvent boundaryEvent, TimerEventDefinition timerEventDefinition, bool interrupting)
        {
            return new BoundaryTimerEventActivityBehavior(timerEventDefinition, interrupting);
        }

        public virtual BoundarySignalEventActivityBehavior CreateBoundarySignalEventActivityBehavior(BoundaryEvent boundaryEvent, SignalEventDefinition signalEventDefinition, Signal signal, bool interrupting)
        {
            return new BoundarySignalEventActivityBehavior(signalEventDefinition, signal, interrupting);
        }

        public virtual BoundaryMessageEventActivityBehavior CreateBoundaryMessageEventActivityBehavior(BoundaryEvent boundaryEvent, MessageEventDefinition messageEventDefinition, bool interrupting)
        {
            return new BoundaryMessageEventActivityBehavior(messageEventDefinition, interrupting);
        }
    }

}