using System;
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

namespace org.activiti.engine.impl.cmd
{

    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.query;
    using org.activiti.engine.task;
    using System.Linq;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    [Serializable]
    public class PageQueryCmd<T, TResult> : ICommand<IList<TResult>>
    {
        protected readonly IQuery<T, TResult> query;
        protected readonly int firstResult;
        protected readonly int pageSize;

        public PageQueryCmd(IQuery<T, TResult> query, int firstResult, int pageSize)
        {
            this.query = query;
            this.firstResult = firstResult;
            this.pageSize = pageSize;
        }

        public virtual IList<TResult> Execute(ICommandContext commandContext)
        {
            return query.ListPage(firstResult, pageSize);
        }
    }
}