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
namespace Sys.Workflow.engine.task
{
    /// <summary>
    /// This is a helper class to help you work with the <seealso cref="TaskInfoQuery"/>, without having to care about the awful generics.
    /// 
    /// Example usage:
    /// 
    /// TaskInfoQueryWrapper taskInfoQueryWrapper = new TaskInfoQueryWrapper(taskService.createTaskQuery()); List<? extends TaskInfo> taskInfos = taskInfoQueryWrapper.getTaskInfoQuery().or()
    /// .taskNameLike("%task%") .taskDescriptionLike("%blah%"); .endOr() .list();
    /// 
    /// First line can be switched to TaskInfoQueryWrapper taskInfoQueryWrapper = new TaskInfoQueryWrapper(historyService.createTaskQuery()); and the same methods can be used on the result.
    /// 
    /// 
    /// </summary>
    public class TaskInfoQueryWrapper
    {
        protected internal ITaskInfoQuery<ITaskInfoQuery<string, ITaskInfo>, ITaskInfo> taskInfoQuery;

        public TaskInfoQueryWrapper(ITaskInfoQuery<ITaskInfoQuery<string, ITaskInfo>, ITaskInfo> taskInfoQuery)
        {
            this.taskInfoQuery = taskInfoQuery;
        }

        public virtual ITaskInfoQuery<ITaskInfoQuery<string, ITaskInfo>, ITaskInfo> TaskInfoQuery
        {
            get
            {
                return taskInfoQuery;
            }
            set
            {
                this.taskInfoQuery = value;
            }
        }


    }

}