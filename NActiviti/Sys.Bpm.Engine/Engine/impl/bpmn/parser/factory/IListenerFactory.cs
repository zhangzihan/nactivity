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

        ITaskListener createClassDelegateTaskListener(ActivitiListener activitiListener);

        ITaskListener createExpressionTaskListener(ActivitiListener activitiListener);

        ITaskListener createDelegateExpressionTaskListener(ActivitiListener activitiListener);

        IExecutionListener createClassDelegateExecutionListener(ActivitiListener activitiListener);

        IExecutionListener createExpressionExecutionListener(ActivitiListener activitiListener);

        IExecutionListener createDelegateExpressionExecutionListener(ActivitiListener activitiListener);

        ITransactionDependentExecutionListener createTransactionDependentDelegateExpressionExecutionListener(ActivitiListener activitiListener);

        IActivitiEventListener createClassDelegateEventListener(EventListener eventListener);

        IActivitiEventListener createDelegateExpressionEventListener(EventListener eventListener);

        IActivitiEventListener createEventThrowingEventListener(EventListener eventListener);

        ICustomPropertiesResolver createClassDelegateCustomPropertiesResolver(ActivitiListener activitiListener);

        ICustomPropertiesResolver createExpressionCustomPropertiesResolver(ActivitiListener activitiListener);

        ICustomPropertiesResolver createDelegateExpressionCustomPropertiesResolver(ActivitiListener activitiListener);

        ITransactionDependentTaskListener createTransactionDependentDelegateExpressionTaskListener(ActivitiListener activitiListener);

        IActivitiEventListener createCustomTaskCompletedEventListener();
    }
}