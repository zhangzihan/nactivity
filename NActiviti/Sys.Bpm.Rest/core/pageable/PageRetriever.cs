using org.activiti.cloud.services.api.model.converter;
using org.activiti.engine.query;
using org.springframework.data.domain;
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
    //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
    //ORIGINAL LINE: @Component public class PageRetriever
    public class PageRetriever
    {
        public virtual Page<TARGET> loadPage<SOURCE, TARGET, T1>(IQuery<T1, SOURCE> query, Pageable pageable, ModelConverter<SOURCE, TARGET> converter)
        {
            IList<SOURCE> elements = query.listPage(pageable.Offset, pageable.PageSize);
            long count = query.count();
            return new PageImpl<TARGET>(converter.from(elements), pageable, count);
        }
    }
}