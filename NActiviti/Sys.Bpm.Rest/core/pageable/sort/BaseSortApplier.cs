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

using Sys.Workflow.api.runtime.shared.query;
using Sys.Workflow.engine;
using Sys.Workflow.engine.query;

namespace Sys.Workflow.cloud.services.core.pageable.sort
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class BaseSortApplier<T, U> : ISortApplier<T> where T : IQuery<T, U>
    {

        /// <summary>
        /// 
        /// </summary>
        public virtual void ApplySort(T query, Pageable pageable)
        {
            if (pageable.Sort != null && pageable.Sort != Sort.UnSorted())
            {
                ApplyPageableSort(query, pageable.Sort);
            }
            else
            {
                ApplyDefaultSort(query);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal abstract void ApplyDefaultSort(T query);

        /// <summary>
        /// 
        /// </summary>
        private void ApplyPageableSort(T query, Sort sort)
        {
            foreach (Sort.Order order in sort)
            {
                ApplyOrder(query, order);
                ApplyDirection(query, order.Direction);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ApplyOrder(T query, Sort.Order order)
        {
            IQueryProperty property = GetOrderByProperty(order);
            if (property != null)
            {
                query.SetOrderBy(property);
            }
            else
            {
                throw new ActivitiIllegalArgumentException("The property '" + order.Property + "' cannot be used to sort the result.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        protected internal abstract IQueryProperty GetOrderByProperty(Sort.Order order);

        private void ApplyDirection(T query, Sort.Direction direction)
        {
            switch (direction)
            {

                case Sort.Direction.ASC:
                    query.Asc();
                    break;

                case Sort.Direction.DESC:
                    query.Desc();
                    break;
            }
        }
    }

}