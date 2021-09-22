using Sys.Workflow.Api.Runtime.Shared.Query;
using Sys.Workflow.Cloud.Services.Api.Model.Converters;
using Sys.Workflow.Engine.Impl;
using Sys.Workflow.Engine.Impl.Cmd;
using Sys.Workflow.Engine.Impl.Interceptor;
using Sys.Workflow.Engine.Query;
using System;
using System.Collections.Generic;

/*
 * Licensed under the Apache License, Version 2.0 (the "License");
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
 *
 */

namespace Sys.Workflow.Cloud.Services.Core.Pageables
{
    /// <summary>
    /// 
    /// </summary>
    public class PageRetriever
    {
        /// <summary>
        /// 
        /// </summary>
        public virtual IPage<TARGET> LoadPage<SOURCE, TARGET, T1>(IQuery<T1, SOURCE> query, Pageable pageable, IModelConverter<SOURCE, TARGET> converter)
        {
            int firstResult = (pageable.PageNo <= 0 ? 0 : pageable.PageNo - 1) * pageable.PageSize;
            IList<SOURCE> elements = query.ListPage(firstResult, pageable.PageSize);
            long count = query.Count();
            return new PageImpl<TARGET>(converter.From(elements), count);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IPage<TARGET> LoadPage<SOURCE, TARGET, T1>(ServiceImpl service,
            IQuery<T1, SOURCE> query,
            Pageable pageable,
            IModelConverter<SOURCE, TARGET> converter,
            Func<IQuery<T1, SOURCE>, int, int, PageQueryCmd<T1, SOURCE>> createPageQueryCmd)
        {
            int firstResult = (pageable.PageNo <= 0 ? 0 : pageable.PageNo - 1) * pageable.PageSize;

            PageQueryCmd<T1, SOURCE> cmd;
            if (createPageQueryCmd is not null)
            {
                cmd = createPageQueryCmd(query, firstResult, pageable.PageSize);
            }
            else
            {
                cmd = new PageQueryCmd<T1, SOURCE>(query, firstResult, pageable.PageSize);
            }
            var elements = service.CommandExecutor.Execute(cmd);
            long count = query.Count();
            return new PageImpl<TARGET>(converter.From(elements), count);
        }
    }
}