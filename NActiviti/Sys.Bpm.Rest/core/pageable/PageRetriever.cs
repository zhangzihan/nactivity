using org.activiti.api.runtime.shared.query;
using org.activiti.cloud.services.api.model.converter;
using org.activiti.engine.query;
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

namespace org.activiti.cloud.services.core.pageable
{
    /// <summary>
    /// 
    /// </summary>
    public class PageRetriever
    {
        /// <summary>
        /// 
        /// </summary>
        public virtual IPage<TARGET> loadPage<SOURCE, TARGET, T1>(IQuery<T1, SOURCE> query, Pageable pageable, IModelConverter<SOURCE, TARGET> converter)
        {
            int firstResult = (pageable.PageNo - 1) * pageable.PageSize;
            IList<SOURCE> elements = query.listPage(firstResult, pageable.PageSize);
            long count = query.count();
            return new PageImpl<TARGET>(converter.from(elements), count);
        }
    }
}