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
            ValidatorSet validatorSet = new ValidatorSet(ValidatorSetNames_Fields.ACTIVITI_EXECUTABLE_PROCESS);

            validatorSet.addValidator(new AssociationValidator());
            validatorSet.addValidator(new SignalValidator());
            validatorSet.addValidator(new OperationValidator());
            validatorSet.addValidator(new ErrorValidator());
            validatorSet.addValidator(new DataObjectValidator());

            validatorSet.addValidator(new BpmnModelValidator());
            validatorSet.addValidator(new FlowElementValidator());

            validatorSet.addValidator(new StartEventValidator());
            validatorSet.addValidator(new SequenceflowValidator());
            validatorSet.addValidator(new UserTaskValidator());
            validatorSet.addValidator(new ServiceTaskValidator());
            validatorSet.addValidator(new ScriptTaskValidator());
            validatorSet.addValidator(new SendTaskValidator());
            validatorSet.addValidator(new ExclusiveGatewayValidator());
            validatorSet.addValidator(new EventGatewayValidator());
            validatorSet.addValidator(new SubprocessValidator());
            validatorSet.addValidator(new EventSubprocessValidator());
            validatorSet.addValidator(new BoundaryEventValidator());
            validatorSet.addValidator(new IntermediateCatchEventValidator());
            validatorSet.addValidator(new IntermediateThrowEventValidator());
            validatorSet.addValidator(new MessageValidator());
            validatorSet.addValidator(new EventValidator());
            validatorSet.addValidator(new EndEventValidator());

            validatorSet.addValidator(new ExecutionListenerValidator());
            validatorSet.addValidator(new ActivitiEventListenerValidator());

            validatorSet.addValidator(new DiagramInterchangeInfoValidator());

            return validatorSet;
        }

    }

}