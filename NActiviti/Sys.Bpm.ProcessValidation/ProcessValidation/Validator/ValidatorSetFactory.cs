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
namespace org.activiti.validation.validator
{
    using org.activiti.validation.validator.impl;

    /// 
    public class ValidatorSetFactory
    {

        public virtual ValidatorSet createActivitiExecutableProcessValidatorSet()
        {
            ValidatorSet validatorSet = new ValidatorSet(ValidatorSetNamesFields.ACTIVITI_EXECUTABLE_PROCESS);

            validatorSet.AddValidator(new AssociationValidator());
            validatorSet.AddValidator(new SignalValidator());
            validatorSet.AddValidator(new OperationValidator());
            validatorSet.AddValidator(new ErrorValidator());
            validatorSet.AddValidator(new DataObjectValidator());

            validatorSet.AddValidator(new BpmnModelValidator());
            validatorSet.AddValidator(new FlowElementValidator());

            validatorSet.AddValidator(new StartEventValidator());
            validatorSet.AddValidator(new SequenceflowValidator());
            validatorSet.AddValidator(new UserTaskValidator());
            validatorSet.AddValidator(new ServiceTaskValidator());
            validatorSet.AddValidator(new ScriptTaskValidator());
            validatorSet.AddValidator(new SendTaskValidator());
            validatorSet.AddValidator(new ExclusiveGatewayValidator());
            validatorSet.AddValidator(new EventGatewayValidator());
            validatorSet.AddValidator(new SubprocessValidator());
            validatorSet.AddValidator(new EventSubprocessValidator());
            validatorSet.AddValidator(new BoundaryEventValidator());
            validatorSet.AddValidator(new IntermediateCatchEventValidator());
            validatorSet.AddValidator(new IntermediateThrowEventValidator());
            validatorSet.AddValidator(new MessageValidator());
            validatorSet.AddValidator(new EventValidator());
            validatorSet.AddValidator(new EndEventValidator());

            validatorSet.AddValidator(new ExecutionListenerValidator());
            validatorSet.AddValidator(new ActivitiEventListenerValidator());

            validatorSet.AddValidator(new DiagramInterchangeInfoValidator());

            return validatorSet;
        }

    }

}