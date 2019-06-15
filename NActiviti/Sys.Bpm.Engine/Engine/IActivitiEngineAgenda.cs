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
namespace org.activiti.engine
{
    using org.activiti.engine.impl.persistence.entity;
    using System.Collections.Generic;

    /// <summary>
    /// This class extends <seealso cref="IAgenda"/> with activiti specific operations
    /// </summary>
    public interface IActivitiEngineAgenda : IAgenda
    {

        void PlanContinueProcessOperation(IExecutionEntity execution);

        void PlanContinueProcessSynchronousOperation(IExecutionEntity execution);

        void PlanContinueProcessInCompensation(IExecutionEntity execution);

        void PlanContinueMultiInstanceOperation(IExecutionEntity execution);

        void PlanTakeOutgoingSequenceFlowsOperation(IExecutionEntity execution, bool evaluateConditions);

        void PlanEndExecutionOperation(IExecutionEntity execution);

        void PlanTriggerExecutionOperation(IExecutionEntity execution);

        void PlanTriggerExecutionOperation(IExecutionEntity execution, object signalData);

        void PlanDestroyScopeOperation(IExecutionEntity execution);

        void PlanExecuteInactiveBehaviorsOperation();
    }

}