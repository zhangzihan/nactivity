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
    using org.activiti.engine.impl.bpmn.behavior;
    using org.activiti.engine.impl.bpmn.helper;
    using org.activiti.engine.impl.@delegate;

    /// <summary>
    /// Factory class used by the <seealso cref="BpmnParser"/> and <seealso cref="BpmnParse"/> to instantiate the behaviour classes. For example when parsing an exclusive gateway, this factory will be requested to create a
    /// new <seealso cref="IActivityBehavior"/> that will be set on the <seealso cref="ActivityImpl"/> of that step of the process and will implement the spec-compliant behavior of the exclusive gateway.
    /// 
    /// You can provide your own implementation of this class. This way, you can give different execution semantics to a standard bpmn xml construct. Eg. you could tweak the exclusive gateway to do
    /// something completely different if you would want that. Creating your own <seealso cref="IActivityBehaviorFactory"/> is only advisable if you want to change the default behavior of any BPMN default construct.
    /// And even then, think twice, because it won't be spec compliant bpmn anymore.
    /// 
    /// Note that you can always express any custom step as a service task with a class delegation.
    /// 
    /// The easiest and advisable way to implement your own <seealso cref="IActivityBehaviorFactory"/> is to extend the <seealso cref="DefaultActivityBehaviorFactory"/> class and override the method specific to the
    /// <seealso cref="IActivityBehavior"/> you want to change.
    /// 
    /// An instance of this interface can be injected in the <seealso cref="ProcessEngineConfigurationImpl"/> and its subclasses.
    /// 
    /// 
    /// </summary>
    public interface IActivityBehaviorFactory
    {

        NoneStartEventActivityBehavior createNoneStartEventActivityBehavior(StartEvent startEvent);

        TaskActivityBehavior createTaskActivityBehavior(Task task);

        ManualTaskActivityBehavior createManualTaskActivityBehavior(ManualTask manualTask);

        ReceiveTaskActivityBehavior createReceiveTaskActivityBehavior(ReceiveTask receiveTask);

        UserTaskActivityBehavior createUserTaskActivityBehavior(UserTask userTask);

        ClassDelegate createClassDelegateServiceTask(ServiceTask serviceTask);

        ServiceTaskDelegateExpressionActivityBehavior createServiceTaskDelegateExpressionActivityBehavior(ServiceTask serviceTask);

        IActivityBehavior createDefaultServiceTaskBehavior(ServiceTask serviceTask);

        ServiceTaskExpressionActivityBehavior createServiceTaskExpressionActivityBehavior(ServiceTask serviceTask);

        WebServiceActivityBehavior createWebServiceActivityBehavior(ServiceTask serviceTask);

        WebServiceActivityBehavior createWebServiceActivityBehavior(SendTask sendTask);

        MailActivityBehavior createMailActivityBehavior(ServiceTask serviceTask);

        MailActivityBehavior createMailActivityBehavior(SendTask sendTask);

        // We do not want a hard dependency on the Mule module, hence we return
        // ActivityBehavior and instantiate the delegate instance using a string instead of the Class itself.
        IActivityBehavior createMuleActivityBehavior(ServiceTask serviceTask);

        IActivityBehavior createMuleActivityBehavior(SendTask sendTask);

        IActivityBehavior createCamelActivityBehavior(ServiceTask serviceTask);

        IActivityBehavior createCamelActivityBehavior(SendTask sendTask);

        ShellActivityBehavior createShellActivityBehavior(ServiceTask serviceTask);

        IActivityBehavior createBusinessRuleTaskActivityBehavior(BusinessRuleTask businessRuleTask);

        ScriptTaskActivityBehavior createScriptTaskActivityBehavior(ScriptTask scriptTask);

        ExclusiveGatewayActivityBehavior createExclusiveGatewayActivityBehavior(ExclusiveGateway exclusiveGateway);

        ParallelGatewayActivityBehavior createParallelGatewayActivityBehavior(ParallelGateway parallelGateway);

        InclusiveGatewayActivityBehavior createInclusiveGatewayActivityBehavior(InclusiveGateway inclusiveGateway);

        EventBasedGatewayActivityBehavior createEventBasedGatewayActivityBehavior(EventGateway eventGateway);

        SequentialMultiInstanceBehavior createSequentialMultiInstanceBehavior(Activity activity, AbstractBpmnActivityBehavior innerActivityBehavior);

        ParallelMultiInstanceBehavior createParallelMultiInstanceBehavior(Activity activity, AbstractBpmnActivityBehavior innerActivityBehavior);

        SubProcessActivityBehavior createSubprocessActivityBehavior(SubProcess subProcess);

        EventSubProcessErrorStartEventActivityBehavior createEventSubProcessErrorStartEventActivityBehavior(StartEvent startEvent);

        EventSubProcessMessageStartEventActivityBehavior createEventSubProcessMessageStartEventActivityBehavior(StartEvent startEvent, MessageEventDefinition messageEventDefinition);

        AdhocSubProcessActivityBehavior createAdhocSubprocessActivityBehavior(SubProcess subProcess);

        CallActivityBehavior createCallActivityBehavior(CallActivity callActivity);

        TransactionActivityBehavior createTransactionActivityBehavior(Transaction transaction);

        IntermediateCatchEventActivityBehavior createIntermediateCatchEventActivityBehavior(IntermediateCatchEvent intermediateCatchEvent);

        IntermediateCatchMessageEventActivityBehavior createIntermediateCatchMessageEventActivityBehavior(IntermediateCatchEvent intermediateCatchEvent, MessageEventDefinition messageEventDefinition);

        IntermediateCatchTimerEventActivityBehavior createIntermediateCatchTimerEventActivityBehavior(IntermediateCatchEvent intermediateCatchEvent, TimerEventDefinition timerEventDefinition);

        IntermediateCatchSignalEventActivityBehavior createIntermediateCatchSignalEventActivityBehavior(IntermediateCatchEvent intermediateCatchEvent, SignalEventDefinition signalEventDefinition, Signal signal);

        IntermediateThrowNoneEventActivityBehavior createIntermediateThrowNoneEventActivityBehavior(ThrowEvent throwEvent);

        IntermediateThrowSignalEventActivityBehavior createIntermediateThrowSignalEventActivityBehavior(ThrowEvent throwEvent, SignalEventDefinition signalEventDefinition, Signal signal);

        IntermediateThrowCompensationEventActivityBehavior createIntermediateThrowCompensationEventActivityBehavior(ThrowEvent throwEvent, CompensateEventDefinition compensateEventDefinition);

        NoneEndEventActivityBehavior createNoneEndEventActivityBehavior(EndEvent endEvent);

        ErrorEndEventActivityBehavior createErrorEndEventActivityBehavior(EndEvent endEvent, ErrorEventDefinition errorEventDefinition);

        CancelEndEventActivityBehavior createCancelEndEventActivityBehavior(EndEvent endEvent);

        TerminateEndEventActivityBehavior createTerminateEndEventActivityBehavior(EndEvent endEvent);

        BoundaryEventActivityBehavior createBoundaryEventActivityBehavior(BoundaryEvent boundaryEvent, bool interrupting);

        BoundaryCancelEventActivityBehavior createBoundaryCancelEventActivityBehavior(CancelEventDefinition cancelEventDefinition);

        BoundaryTimerEventActivityBehavior createBoundaryTimerEventActivityBehavior(BoundaryEvent boundaryEvent, TimerEventDefinition timerEventDefinition, bool interrupting);

        BoundarySignalEventActivityBehavior createBoundarySignalEventActivityBehavior(BoundaryEvent boundaryEvent, SignalEventDefinition signalEventDefinition, Signal signal, bool interrupting);

        BoundaryMessageEventActivityBehavior createBoundaryMessageEventActivityBehavior(BoundaryEvent boundaryEvent, MessageEventDefinition messageEventDefinition, bool interrupting);

        BoundaryCompensateEventActivityBehavior createBoundaryCompensateEventActivityBehavior(BoundaryEvent boundaryEvent, CompensateEventDefinition compensateEventDefinition, bool interrupting);
    }
}