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
    using org.activiti.engine.impl.bpmn.behavior;
    using org.activiti.engine.impl.bpmn.helper;
    using org.activiti.engine.impl.@delegate;
    using org.activiti.engine.impl.scripting;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Default implementation of the <seealso cref="IActivityBehaviorFactory"/>. Used when no custom <seealso cref="IActivityBehaviorFactory"/> is injected on the <seealso cref="ProcessEngineConfigurationImpl"/>.
    /// </summary>
    public class DefaultActivityBehaviorFactory : AbstractBehaviorFactory, IActivityBehaviorFactory
    {

        public const string DEFAULT_SERVICE_TASK_BEAN_NAME = "defaultServiceTaskBehavior";
        private readonly IClassDelegateFactory classDelegateFactory;

        public DefaultActivityBehaviorFactory(IClassDelegateFactory classDelegateFactory)
        {
            this.classDelegateFactory = classDelegateFactory;
        }

        public DefaultActivityBehaviorFactory() : this(new DefaultClassDelegateFactory())
        {
        }

        // Start event
        public const string EXCEPTION_MAP_FIELD = "mapExceptions";

        public virtual NoneStartEventActivityBehavior createNoneStartEventActivityBehavior(StartEvent startEvent)
        {
            return new NoneStartEventActivityBehavior();
        }

        // Task

        public virtual TaskActivityBehavior createTaskActivityBehavior(Task task)
        {
            return new TaskActivityBehavior();
        }

        public virtual ManualTaskActivityBehavior createManualTaskActivityBehavior(ManualTask manualTask)
        {
            return new ManualTaskActivityBehavior();
        }

        public virtual ReceiveTaskActivityBehavior createReceiveTaskActivityBehavior(ReceiveTask receiveTask)
        {
            return new ReceiveTaskActivityBehavior();
        }

        public virtual UserTaskActivityBehavior createUserTaskActivityBehavior(UserTask userTask)
        {
            return new UserTaskActivityBehavior(userTask);
        }

        // Service task

        protected internal virtual IExpression getSkipExpressionFromServiceTask(ServiceTask serviceTask)
        {
            IExpression result = null;
            if (!string.IsNullOrWhiteSpace(serviceTask.SkipExpression))
            {
                result = expressionManager.createExpression(serviceTask.SkipExpression);
            }
            return result;
        }

        public virtual ClassDelegate createClassDelegateServiceTask(ServiceTask serviceTask)
        {
            return classDelegateFactory.create(serviceTask.Id, serviceTask.Implementation, createFieldDeclarations(serviceTask.FieldExtensions), getSkipExpressionFromServiceTask(serviceTask), serviceTask.MapExceptions);
        }

        public virtual ServiceTaskDelegateExpressionActivityBehavior createServiceTaskDelegateExpressionActivityBehavior(ServiceTask serviceTask)
        {
            IExpression delegateExpression = expressionManager.createExpression(serviceTask.Implementation);
            return createServiceTaskBehavior(serviceTask, delegateExpression);
        }

        public virtual IActivityBehavior createDefaultServiceTaskBehavior(ServiceTask serviceTask)
        {
            // this is covering the case where only the field `implementation` was defined in the process definition. I.e.
            // <serviceTask id="serviceTask" implementation="myServiceTaskImpl"/>
            // `myServiceTaskImpl` can be different things depending on the implementation of `defaultServiceTaskBehavior`
            // could be for instance a Spring bean or a target for a Spring Stream
            IExpression delegateExpression = expressionManager.createExpression("${" + DEFAULT_SERVICE_TASK_BEAN_NAME + "}");
            return createServiceTaskBehavior(serviceTask, delegateExpression);
        }

        private ServiceTaskDelegateExpressionActivityBehavior createServiceTaskBehavior(ServiceTask serviceTask, IExpression delegateExpression)
        {
            return new ServiceTaskDelegateExpressionActivityBehavior(serviceTask.Id, delegateExpression, getSkipExpressionFromServiceTask(serviceTask), createFieldDeclarations(serviceTask.FieldExtensions));
        }

        public virtual ServiceTaskExpressionActivityBehavior createServiceTaskExpressionActivityBehavior(ServiceTask serviceTask)
        {
            IExpression expression = expressionManager.createExpression(serviceTask.Implementation);
            return new ServiceTaskExpressionActivityBehavior(serviceTask.Id, expression, getSkipExpressionFromServiceTask(serviceTask), serviceTask.ResultVariableName);
        }

        public virtual WebServiceActivityBehavior createWebServiceActivityBehavior(ServiceTask serviceTask)
        {
            return new WebServiceActivityBehavior();
        }

        public virtual WebServiceActivityBehavior createWebServiceActivityBehavior(SendTask sendTask)
        {
            return new WebServiceActivityBehavior();
        }

        public virtual MailActivityBehavior createMailActivityBehavior(ServiceTask serviceTask)
        {
            return createMailActivityBehavior(serviceTask.Id, serviceTask.FieldExtensions);
        }

        public virtual MailActivityBehavior createMailActivityBehavior(SendTask sendTask)
        {
            return createMailActivityBehavior(sendTask.Id, sendTask.FieldExtensions);
        }

        protected internal virtual MailActivityBehavior createMailActivityBehavior(string taskId, IList<FieldExtension> fields)
        {
            IList<FieldDeclaration> fieldDeclarations = createFieldDeclarations(fields);
            return (MailActivityBehavior)ClassDelegate.defaultInstantiateDelegate(typeof(MailActivityBehavior), fieldDeclarations);
        }

        // We do not want a hard dependency on Mule, hence we return
        // ActivityBehavior and instantiate the delegate instance using a string instead of the Class itself.
        public virtual IActivityBehavior createMuleActivityBehavior(ServiceTask serviceTask)
        {
            return createMuleActivityBehavior(serviceTask, serviceTask.FieldExtensions);
        }

        public virtual IActivityBehavior createMuleActivityBehavior(SendTask sendTask)
        {
            return createMuleActivityBehavior(sendTask, sendTask.FieldExtensions);
        }

        protected internal virtual IActivityBehavior createMuleActivityBehavior(TaskWithFieldExtensions task, IList<FieldExtension> fieldExtensions)
        {
            try
            {

                Type theClass = Type.GetType("org.activiti.mule.MuleSendActivitiBehavior");
                IList<FieldDeclaration> fieldDeclarations = createFieldDeclarations(fieldExtensions);
                return (IActivityBehavior)ClassDelegate.defaultInstantiateDelegate(theClass, fieldDeclarations);
            }
            catch (Exception e)
            {
                throw new ActivitiException("Could not find org.activiti.mule.MuleSendActivitiBehavior: ", e);
            }
        }

        // We do not want a hard dependency on Camel, hence we return
        // ActivityBehavior and instantiate the delegate instance using a string instead of the Class itself.
        public virtual IActivityBehavior createCamelActivityBehavior(ServiceTask serviceTask)
        {
            return createCamelActivityBehavior(serviceTask, serviceTask.FieldExtensions);
        }

        public virtual IActivityBehavior createCamelActivityBehavior(SendTask sendTask)
        {
            return createCamelActivityBehavior(sendTask, sendTask.FieldExtensions);
        }

        protected internal virtual IActivityBehavior createCamelActivityBehavior(TaskWithFieldExtensions task, IList<FieldExtension> fieldExtensions)
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
                    theClass = Type.GetType("org.activiti.camel.impl.CamelBehaviorDefaultImpl");
                }

                IList<FieldDeclaration> fieldDeclarations = createFieldDeclarations(fieldExtensions);
                addExceptionMapAsFieldDeclaration(fieldDeclarations, task.MapExceptions);
                return (IActivityBehavior)ClassDelegate.defaultInstantiateDelegate(theClass, fieldDeclarations);
            }
            catch (Exception e)
            {
                throw new ActivitiException("Could not find org.activiti.camel.CamelBehavior: ", e);
            }
        }

        private void addExceptionMapAsFieldDeclaration(IList<FieldDeclaration> fieldDeclarations, IList<MapExceptionEntry> mapExceptions)
        {
            FieldDeclaration exceptionMapsFieldDeclaration = new FieldDeclaration(EXCEPTION_MAP_FIELD, mapExceptions.GetType().ToString(), mapExceptions);
            fieldDeclarations.Add(exceptionMapsFieldDeclaration);
        }

        public virtual ShellActivityBehavior createShellActivityBehavior(ServiceTask serviceTask)
        {
            IList<FieldDeclaration> fieldDeclarations = createFieldDeclarations(serviceTask.FieldExtensions);
            return (ShellActivityBehavior)ClassDelegate.defaultInstantiateDelegate(typeof(ShellActivityBehavior), fieldDeclarations);
        }

        public virtual IActivityBehavior createBusinessRuleTaskActivityBehavior(BusinessRuleTask businessRuleTask)
        {
            IBusinessRuleTaskDelegate ruleActivity = null;
            if (!string.IsNullOrWhiteSpace(businessRuleTask.ClassName))
            {
                try
                {
                    Type clazz = Type.GetType(businessRuleTask.ClassName);
                    ruleActivity = (IBusinessRuleTaskDelegate)System.Activator.CreateInstance(clazz);
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
                ruleActivity.addRuleVariableInputIdExpression(expressionManager.createExpression(ruleVariableInputObject.Trim()));
            }

            foreach (string rule in businessRuleTask.RuleNames)
            {
                ruleActivity.addRuleIdExpression(expressionManager.createExpression(rule.Trim()));
            }

            ruleActivity.Exclude = businessRuleTask.Exclude;

            if (!string.ReferenceEquals(businessRuleTask.ResultVariableName, null) && businessRuleTask.ResultVariableName.Length > 0)
            {
                ruleActivity.ResultVariable = businessRuleTask.ResultVariableName;
            }
            else
            {
                ruleActivity.ResultVariable = "org.activiti.engine.rules.OUTPUT";
            }

            return ruleActivity;
        }

        // Script task

        public virtual ScriptTaskActivityBehavior createScriptTaskActivityBehavior(ScriptTask scriptTask)
        {
            string language = scriptTask.ScriptFormat;
            if (string.ReferenceEquals(language, null))
            {
                //language = ScriptingEngines.DEFAULT_SCRIPTING_LANGUAGE;
            }
            return new ScriptTaskActivityBehavior(scriptTask.Id, scriptTask.Script, language, scriptTask.ResultVariable, scriptTask.AutoStoreVariables);
        }

        // Gateways

        public virtual ExclusiveGatewayActivityBehavior createExclusiveGatewayActivityBehavior(ExclusiveGateway exclusiveGateway)
        {
            return new ExclusiveGatewayActivityBehavior();
        }

        public virtual ParallelGatewayActivityBehavior createParallelGatewayActivityBehavior(ParallelGateway parallelGateway)
        {
            return new ParallelGatewayActivityBehavior();
        }

        public virtual InclusiveGatewayActivityBehavior createInclusiveGatewayActivityBehavior(InclusiveGateway inclusiveGateway)
        {
            return new InclusiveGatewayActivityBehavior();
        }

        public virtual EventBasedGatewayActivityBehavior createEventBasedGatewayActivityBehavior(EventGateway eventGateway)
        {
            return new EventBasedGatewayActivityBehavior();
        }

        // Multi Instance

        public virtual SequentialMultiInstanceBehavior createSequentialMultiInstanceBehavior(Activity activity, AbstractBpmnActivityBehavior innerActivityBehavior)
        {
            return new SequentialMultiInstanceBehavior(activity, innerActivityBehavior);
        }

        public virtual ParallelMultiInstanceBehavior createParallelMultiInstanceBehavior(Activity activity, AbstractBpmnActivityBehavior innerActivityBehavior)
        {
            return new ParallelMultiInstanceBehavior(activity, innerActivityBehavior);
        }

        // Subprocess

        public virtual SubProcessActivityBehavior createSubprocessActivityBehavior(SubProcess subProcess)
        {
            return new SubProcessActivityBehavior();
        }

        public virtual EventSubProcessErrorStartEventActivityBehavior createEventSubProcessErrorStartEventActivityBehavior(StartEvent startEvent)
        {
            return new EventSubProcessErrorStartEventActivityBehavior();
        }

        public virtual EventSubProcessMessageStartEventActivityBehavior createEventSubProcessMessageStartEventActivityBehavior(StartEvent startEvent, MessageEventDefinition messageEventDefinition)
        {
            return new EventSubProcessMessageStartEventActivityBehavior(messageEventDefinition);
        }

        public virtual AdhocSubProcessActivityBehavior createAdhocSubprocessActivityBehavior(SubProcess subProcess)
        {
            return new AdhocSubProcessActivityBehavior();
        }

        // Call activity

        public virtual CallActivityBehavior createCallActivityBehavior(CallActivity callActivity)
        {
            string expressionRegex = "\\$+\\{+.+\\}";

            CallActivityBehavior callActivityBehaviour = null;
            if (!string.IsNullOrWhiteSpace(callActivity.CalledElement) && new Regex(expressionRegex).IsMatch(callActivity.CalledElement))
            {
                callActivityBehaviour = new CallActivityBehavior(expressionManager.createExpression(callActivity.CalledElement), callActivity.MapExceptions);
            }
            else
            {
                callActivityBehaviour = new CallActivityBehavior(callActivity.CalledElement, callActivity.MapExceptions);
            }

            return callActivityBehaviour;
        }

        // Transaction

        public virtual TransactionActivityBehavior createTransactionActivityBehavior(Transaction transaction)
        {
            return new TransactionActivityBehavior();
        }

        // Intermediate Events

        public virtual IntermediateCatchEventActivityBehavior createIntermediateCatchEventActivityBehavior(IntermediateCatchEvent intermediateCatchEvent)
        {
            return new IntermediateCatchEventActivityBehavior();
        }

        public virtual IntermediateCatchMessageEventActivityBehavior createIntermediateCatchMessageEventActivityBehavior(IntermediateCatchEvent intermediateCatchEvent, MessageEventDefinition messageEventDefinition)
        {
            return new IntermediateCatchMessageEventActivityBehavior(messageEventDefinition);
        }

        public virtual IntermediateCatchTimerEventActivityBehavior createIntermediateCatchTimerEventActivityBehavior(IntermediateCatchEvent intermediateCatchEvent, TimerEventDefinition timerEventDefinition)
        {
            return new IntermediateCatchTimerEventActivityBehavior(timerEventDefinition);
        }

        public virtual IntermediateCatchSignalEventActivityBehavior createIntermediateCatchSignalEventActivityBehavior(IntermediateCatchEvent intermediateCatchEvent, SignalEventDefinition signalEventDefinition, Signal signal)
        {

            return new IntermediateCatchSignalEventActivityBehavior(signalEventDefinition, signal);
        }

        public virtual IntermediateThrowNoneEventActivityBehavior createIntermediateThrowNoneEventActivityBehavior(ThrowEvent throwEvent)
        {
            return new IntermediateThrowNoneEventActivityBehavior();
        }

        public virtual IntermediateThrowSignalEventActivityBehavior createIntermediateThrowSignalEventActivityBehavior(ThrowEvent throwEvent, SignalEventDefinition signalEventDefinition, Signal signal)
        {

            return new IntermediateThrowSignalEventActivityBehavior(signalEventDefinition, signal);
        }

        public virtual IntermediateThrowCompensationEventActivityBehavior createIntermediateThrowCompensationEventActivityBehavior(ThrowEvent throwEvent, CompensateEventDefinition compensateEventDefinition)
        {
            return new IntermediateThrowCompensationEventActivityBehavior(compensateEventDefinition);
        }

        // End events

        public virtual NoneEndEventActivityBehavior createNoneEndEventActivityBehavior(EndEvent endEvent)
        {
            return new NoneEndEventActivityBehavior();
        }

        public virtual ErrorEndEventActivityBehavior createErrorEndEventActivityBehavior(EndEvent endEvent, ErrorEventDefinition errorEventDefinition)
        {
            return new ErrorEndEventActivityBehavior(errorEventDefinition.ErrorCode);
        }

        public virtual CancelEndEventActivityBehavior createCancelEndEventActivityBehavior(EndEvent endEvent)
        {
            return new CancelEndEventActivityBehavior();
        }

        public virtual TerminateEndEventActivityBehavior createTerminateEndEventActivityBehavior(EndEvent endEvent)
        {
            bool terminateAll = false;
            bool terminateMultiInstance = false;

            if (endEvent.EventDefinitions != null && endEvent.EventDefinitions.Count > 0 && endEvent.EventDefinitions[0] is TerminateEventDefinition)
            {
                terminateAll = ((TerminateEventDefinition)endEvent.EventDefinitions[0]).TerminateAll;
                terminateMultiInstance = ((TerminateEventDefinition)endEvent.EventDefinitions[0]).TerminateMultiInstance;
            }

            TerminateEndEventActivityBehavior terminateEndEventActivityBehavior = new TerminateEndEventActivityBehavior();
            terminateEndEventActivityBehavior.TerminateAll = terminateAll;
            terminateEndEventActivityBehavior.TerminateMultiInstance = terminateMultiInstance;
            return terminateEndEventActivityBehavior;
        }

        // Boundary Events

        public virtual BoundaryEventActivityBehavior createBoundaryEventActivityBehavior(BoundaryEvent boundaryEvent, bool interrupting)
        {
            return new BoundaryEventActivityBehavior(interrupting);
        }

        public virtual BoundaryCancelEventActivityBehavior createBoundaryCancelEventActivityBehavior(CancelEventDefinition cancelEventDefinition)
        {
            return new BoundaryCancelEventActivityBehavior();
        }

        public virtual BoundaryCompensateEventActivityBehavior createBoundaryCompensateEventActivityBehavior(BoundaryEvent boundaryEvent, CompensateEventDefinition compensateEventDefinition, bool interrupting)
        {

            return new BoundaryCompensateEventActivityBehavior(compensateEventDefinition, interrupting);
        }

        public virtual BoundaryTimerEventActivityBehavior createBoundaryTimerEventActivityBehavior(BoundaryEvent boundaryEvent, TimerEventDefinition timerEventDefinition, bool interrupting)
        {
            return new BoundaryTimerEventActivityBehavior(timerEventDefinition, interrupting);
        }

        public virtual BoundarySignalEventActivityBehavior createBoundarySignalEventActivityBehavior(BoundaryEvent boundaryEvent, SignalEventDefinition signalEventDefinition, Signal signal, bool interrupting)
        {
            return new BoundarySignalEventActivityBehavior(signalEventDefinition, signal, interrupting);
        }

        public virtual BoundaryMessageEventActivityBehavior createBoundaryMessageEventActivityBehavior(BoundaryEvent boundaryEvent, MessageEventDefinition messageEventDefinition, bool interrupting)
        {
            return new BoundaryMessageEventActivityBehavior(messageEventDefinition, interrupting);
        }
    }

}