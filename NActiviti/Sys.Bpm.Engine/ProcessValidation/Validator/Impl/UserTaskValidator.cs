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
namespace org.activiti.validation.validator.impl
{

    using org.activiti.bpmn.model;

    /// 
    public class UserTaskValidator : ProcessLevelValidator
    {

        protected internal override void executeValidation(BpmnModel bpmnModel, Process process, IList<ValidationError> errors)
        {
            IList<UserTask> userTasks = process.findFlowElementsOfType<UserTask>();
            foreach (UserTask userTask in userTasks)
            {
                if (userTask.TaskListeners != null)
                {
                    foreach (ActivitiListener listener in userTask.TaskListeners)
                    {
                        if (ReferenceEquals(listener.Implementation, null) || ReferenceEquals(listener.ImplementationType, null))
                        {
                            addError(errors, Problems_Fields.USER_TASK_LISTENER_IMPLEMENTATION_MISSING, process, userTask, "Element 'class' or 'expression' is mandatory on executionListener");
                        }
                    }
                }
            }
        }

    }

}