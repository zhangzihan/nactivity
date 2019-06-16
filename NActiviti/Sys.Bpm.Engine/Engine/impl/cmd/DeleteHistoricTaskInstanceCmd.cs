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

namespace Sys.Workflow.engine.impl.cmd
{

    using Sys.Workflow.engine.impl.interceptor;
    using System.Collections.Generic;

    /// 
    [Serializable]
    public class DeleteHistoricTaskInstanceCmd : ICommand<object>
    {

        private const long serialVersionUID = 1L;
        protected internal string taskId;

        public DeleteHistoricTaskInstanceCmd(string taskId)
        {
            this.taskId = taskId;
        }

        public virtual object Execute(ICommandContext commandContext)
        {
            if (taskId is null)
            {
                throw new ActivitiIllegalArgumentException("taskId is null");
            }
            commandContext.HistoricTaskInstanceEntityManager.Delete(new KeyValuePair<string, object>("id", taskId));

            return null;
        }

    }

}