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
    using Sys.Workflow.Engine.Delegate;
    using Sys.Workflow.Engine.Delegate.Events;

    /// <summary>
    /// Factory class used by the <seealso cref="BpmnParser"/> and <seealso cref="BpmnParse"/> to instantiate the behaviour classes for <seealso cref="ITaskListener"/> and <seealso cref="IExecutionListener"/> usages.
    /// 
    /// You can provide your own implementation of this class. This way, you can give different execution semantics to the standard construct.
    /// 
    /// The easiest and advisable way to implement your own <seealso cref="IListenerFactory"/> is to extend the <seealso cref="DefaultListenerFactory"/>.
    /// 
    /// An instance of this interface can be injected in the <seealso cref="ProcessEngineConfigurationImpl"/> and its subclasses.
    /// 
    /// 
    /// 
    /// </summary>
    public interface IListenerFactory
    {

        ITaskListener CreateClassDelegateTaskListener(ActivitiListener activitiListener);

        ITaskListener CreateExpressionTaskListener(ActivitiListener activitiListener);

        ITaskListener CreateDelegateExpressionTaskListener(ActivitiListener activitiListener);

        IExecutionListener CreateClassDelegateExecutionListener(ActivitiListener activitiListener);

        IExecutionListener CreateExpressionExecutionListener(ActivitiListener activitiListener);

        IExecutionListener CreateDelegateExpressionExecutionListener(ActivitiListener activitiListener);

        ITransactionDependentExecutionListener CreateTransactionDependentDelegateExpressionExecutionListener(ActivitiListener activitiListener);

        IActivitiEventListener CreateClassDelegateEventListener(EventListener eventListener);

        IActivitiEventListener CreateDelegateExpressionEventListener(EventListener eventListener);

        IActivitiEventListener CreateEventThrowingEventListener(EventListener eventListener);

        ICustomPropertiesResolver CreateClassDelegateCustomPropertiesResolver(ActivitiListener activitiListener);

        ICustomPropertiesResolver CreateExpressionCustomPropertiesResolver(ActivitiListener activitiListener);

        ICustomPropertiesResolver CreateDelegateExpressionCustomPropertiesResolver(ActivitiListener activitiListener);

        ITransactionDependentTaskListener CreateTransactionDependentDelegateExpressionTaskListener(ActivitiListener activitiListener);

        IActivitiEventListener CreateCustomTaskCompletedEventListener();
    }
}