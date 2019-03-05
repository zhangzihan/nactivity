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

using org.activiti.api.runtime.shared.query;
using org.activiti.engine;
using org.activiti.engine.query;

namespace org.activiti.cloud.services.core.pageable.sort
{
    /// 
    public abstract class BaseSortApplier<T, U> : SortApplier<T> where T : IQuery<T, U>
    {

        public virtual void applySort(T query, Pageable pageable)
        {
            if (pageable.Sort != null && pageable.Sort != Sort.unsorted())
            {
                applyPageableSort(query, pageable.Sort);
            }
            else
            {
                applyDefaultSort(query);
            }
        }

        protected internal abstract void applyDefaultSort(T query);

        private void applyPageableSort(T query, Sort sort)
        {
            foreach (Sort.Order order in sort)
            {
                applyOrder(query, order);
                applyDirection(query, order.Direction);
            }
        }

        private void applyOrder(T query, Sort.Order order)
        {
            IQueryProperty property = getOrderByProperty(order);
            if (property != null)
            {
                query.orderBy(property);
            }
            else
            {
                throw new ActivitiIllegalArgumentException("The property '" + order.Property + "' cannot be used to sort the result.");
            }
        }

        protected internal abstract IQueryProperty getOrderByProperty(Sort.Order order);

        private void applyDirection(T query, Sort.Direction direction)
        {
            switch (direction)
            {

                case Sort.Direction.ASC:
                    query.asc();
                    break;

                case Sort.Direction.DESC:
                    query.desc();
                    break;
            }
        }
    }

}