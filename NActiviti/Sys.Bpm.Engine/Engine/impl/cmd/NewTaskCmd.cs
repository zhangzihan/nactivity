﻿using System;

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
namespace Sys.Workflow.Engine.Impl.Cmd
{

    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Runtime;
    using Sys.Workflow.Engine.Tasks;

    /// 
    [Serializable]
    public class NewTaskCmd : ICommand<ITaskEntity>
    {

        private const long serialVersionUID = 1L;

        protected internal string taskId;

        public NewTaskCmd(string taskId)
        {
            this.taskId = taskId;
        }

        public  virtual ITaskEntity  Execute(ICommandContext  commandContext)
        {
            ITaskEntity task = commandContext.TaskEntityManager.Create();
            task.Id = taskId;
            task.CreateTime = commandContext.ProcessEngineConfiguration.Clock.CurrentTime;
            task.Revision = 0;           
            return task;
        }

    }

}