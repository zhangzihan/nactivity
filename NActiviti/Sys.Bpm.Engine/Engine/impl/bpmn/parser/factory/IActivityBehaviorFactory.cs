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
    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine.Impl.Bpmn.Behavior;
    using Sys.Workflow.Engine.Impl.Bpmn.Helper;
    using Sys.Workflow.Engine.Impl.Delegate;

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

        NoneStartEventActivityBehavior CreateNoneStartEventActivityBehavior(StartEvent startEvent);

        TaskActivityBehavior CreateTaskActivityBehavior(TaskActivity task);

        ManualTaskActivityBehavior CreateManualTaskActivityBehavior(ManualTask manualTask);

        ReceiveTaskActivityBehavior CreateReceiveTaskActivityBehavior(ReceiveTask receiveTask);

        UserTaskActivityBehavior CreateUserTaskActivityBehavior(UserTask userTask);

        ClassDelegate CreateClassDelegateSendTask(SendTask serviceTask);

        ClassDelegate CreateClassDelegateServiceTask(ServiceTask serviceTask);

        ServiceTaskDelegateExpressionActivityBehavior CreateServiceTaskDelegateExpressionActivityBehavior(ServiceTask serviceTask);

        IActivityBehavior CreateDefaultServiceTaskBehavior(ServiceTask serviceTask);

        ServiceTaskExpressionActivityBehavior CreateServiceTaskExpressionActivityBehavior(ServiceTask serviceTask);

        WebServiceActivityBehavior CreateWebServiceActivityBehavior(ServiceTask serviceTask);

        WebServiceActivityBehavior CreateWebServiceActivityBehavior(SendTask sendTask);

        MailActivityBehavior CreateMailActivityBehavior(ServiceTask serviceTask);

        MailActivityBehavior CreateMailActivityBehavior(SendTask sendTask);

        // We do not want a hard dependency on the Mule module, hence we return
        // ActivityBehavior and instantiate the delegate instance using a string instead of the Class itself.
        IActivityBehavior CreateMuleActivityBehavior(ServiceTask serviceTask);

        IActivityBehavior CreateMuleActivityBehavior(SendTask sendTask);

        IActivityBehavior CreateCamelActivityBehavior(ServiceTask serviceTask);

        IActivityBehavior CreateCamelActivityBehavior(SendTask sendTask);

        ShellActivityBehavior CreateShellActivityBehavior(ServiceTask serviceTask);

        IActivityBehavior CreateBusinessRuleTaskActivityBehavior(BusinessRuleTask businessRuleTask);

        ScriptTaskActivityBehavior CreateScriptTaskActivityBehavior(ScriptTask scriptTask);

        ExclusiveGatewayActivityBehavior CreateExclusiveGatewayActivityBehavior(ExclusiveGateway exclusiveGateway);

        ParallelGatewayActivityBehavior CreateParallelGatewayActivityBehavior(ParallelGateway parallelGateway);

        InclusiveGatewayActivityBehavior CreateInclusiveGatewayActivityBehavior(InclusiveGateway inclusiveGateway);

        EventBasedGatewayActivityBehavior CreateEventBasedGatewayActivityBehavior(EventGateway eventGateway);

        SequentialMultiInstanceBehavior CreateSequentialMultiInstanceBehavior(Activity activity, AbstractBpmnActivityBehavior innerActivityBehavior);

        ParallelMultiInstanceBehavior CreateParallelMultiInstanceBehavior(Activity activity, AbstractBpmnActivityBehavior innerActivityBehavior);

        SubProcessActivityBehavior CreateSubprocessActivityBehavior(SubProcess subProcess);

        EventSubProcessErrorStartEventActivityBehavior CreateEventSubProcessErrorStartEventActivityBehavior(StartEvent startEvent);

        EventSubProcessMessageStartEventActivityBehavior CreateEventSubProcessMessageStartEventActivityBehavior(StartEvent startEvent, MessageEventDefinition messageEventDefinition);

        AdhocSubProcessActivityBehavior CreateAdhocSubprocessActivityBehavior(SubProcess subProcess);

        CallActivityBehavior CreateCallActivityBehavior(CallActivity callActivity);

        TransactionActivityBehavior CreateTransactionActivityBehavior(Transaction transaction);

        IntermediateCatchEventActivityBehavior CreateIntermediateCatchEventActivityBehavior(IntermediateCatchEvent intermediateCatchEvent);

        IntermediateCatchMessageEventActivityBehavior CreateIntermediateCatchMessageEventActivityBehavior(IntermediateCatchEvent intermediateCatchEvent, MessageEventDefinition messageEventDefinition);

        IntermediateCatchTimerEventActivityBehavior CreateIntermediateCatchTimerEventActivityBehavior(IntermediateCatchEvent intermediateCatchEvent, TimerEventDefinition timerEventDefinition);

        IntermediateCatchSignalEventActivityBehavior CreateIntermediateCatchSignalEventActivityBehavior(IntermediateCatchEvent intermediateCatchEvent, SignalEventDefinition signalEventDefinition, Signal signal);

        IntermediateThrowNoneEventActivityBehavior CreateIntermediateThrowNoneEventActivityBehavior(ThrowEvent throwEvent);

        IntermediateThrowSignalEventActivityBehavior CreateIntermediateThrowSignalEventActivityBehavior(ThrowEvent throwEvent, SignalEventDefinition signalEventDefinition, Signal signal);

        IntermediateThrowCompensationEventActivityBehavior CreateIntermediateThrowCompensationEventActivityBehavior(ThrowEvent throwEvent, CompensateEventDefinition compensateEventDefinition);

        NoneEndEventActivityBehavior CreateNoneEndEventActivityBehavior(EndEvent endEvent);

        ErrorEndEventActivityBehavior CreateErrorEndEventActivityBehavior(EndEvent endEvent, ErrorEventDefinition errorEventDefinition);

        CancelEndEventActivityBehavior CreateCancelEndEventActivityBehavior(EndEvent endEvent);

        TerminateEndEventActivityBehavior CreateTerminateEndEventActivityBehavior(EndEvent endEvent);

        BoundaryEventActivityBehavior CreateBoundaryEventActivityBehavior(BoundaryEvent boundaryEvent, bool interrupting);

        BoundaryCancelEventActivityBehavior CreateBoundaryCancelEventActivityBehavior(CancelEventDefinition cancelEventDefinition);

        BoundaryTimerEventActivityBehavior CreateBoundaryTimerEventActivityBehavior(BoundaryEvent boundaryEvent, TimerEventDefinition timerEventDefinition, bool interrupting);

        BoundarySignalEventActivityBehavior CreateBoundarySignalEventActivityBehavior(BoundaryEvent boundaryEvent, SignalEventDefinition signalEventDefinition, Signal signal, bool interrupting);

        BoundaryMessageEventActivityBehavior CreateBoundaryMessageEventActivityBehavior(BoundaryEvent boundaryEvent, MessageEventDefinition messageEventDefinition, bool interrupting);

        BoundaryCompensateEventActivityBehavior CreateBoundaryCompensateEventActivityBehavior(BoundaryEvent boundaryEvent, CompensateEventDefinition compensateEventDefinition, bool interrupting);
    }
}