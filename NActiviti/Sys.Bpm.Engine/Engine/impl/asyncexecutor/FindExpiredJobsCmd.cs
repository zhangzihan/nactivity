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
namespace org.activiti.engine.impl.asyncexecutor
{

    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;

    /// 
    public class FindExpiredJobsCmd : ICommand<IList<IJobEntity>>
    {

        protected internal int pageSize;

        public FindExpiredJobsCmd(int pageSize)
        {
            this.pageSize = pageSize;
        }

        public virtual IList<IJobEntity> execute(ICommandContext commandContext)
        {
            return commandContext.JobEntityManager.findExpiredJobs(new Page(0, pageSize));
        }

    }

}