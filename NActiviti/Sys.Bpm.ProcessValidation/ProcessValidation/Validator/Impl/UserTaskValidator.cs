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
namespace Sys.Workflow.Validation.Validators.Impl
{
    using Sys.Workflow.Bpmn.Constants;
    using Sys.Workflow.Bpmn.Models;

    /// 
    public class UserTaskValidator : ProcessLevelValidator
    {

        protected internal override void ExecuteValidation(BpmnModel bpmnModel, Process process, IList<ValidationError> errors)
        {
            IList<UserTask> userTasks = process.FindFlowElementsOfType<UserTask>();
            foreach (UserTask userTask in userTasks)
            {
                if (userTask.TaskListeners != null)
                {
                    foreach (ActivitiListener listener in userTask.TaskListeners)
                    {
                        if (string.IsNullOrWhiteSpace(listener.Implementation) || string.IsNullOrWhiteSpace(listener.ImplementationType))
                        {
                            AddError(errors, ProblemsConstants.USER_TASK_LISTENER_IMPLEMENTATION_MISSING, process, userTask, ProcessValidatorResource.EVENT_LISTENER_IMPLEMENTATION_MISSING);
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(userTask.Name) || userTask.Name.Length > Constraints.BPMN_MODEL_NAME_MAX_LENGTH)
                {
                    AddError(errors, ProblemsConstants.USER_TASK_NAME_TOO_LONG, process, userTask, ProcessValidatorResource.NAME_TOO_LONG);
                }
            }
        }
    }
}