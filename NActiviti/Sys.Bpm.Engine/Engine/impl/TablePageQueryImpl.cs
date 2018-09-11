using System;

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
namespace org.activiti.engine.impl
{

    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.management;

    /// 
    /// 
    [Serializable]
    public class TablePageQueryImpl : ITablePageQuery, ICommand<TablePage>
    {

        private const long serialVersionUID = 1L;

        [NonSerialized]
        internal ICommandExecutor commandExecutor;
        protected internal string tableName_Renamed;
        protected internal string order;
        protected internal int firstResult;
        protected internal int maxResults;

        public TablePageQueryImpl()
        {
        }

        public TablePageQueryImpl(ICommandExecutor commandExecutor)
        {
            this.commandExecutor = commandExecutor;
        }

        public virtual ITablePageQuery tableName(string tableName)
        {
            this.tableName_Renamed = tableName;
            return this;
        }

        public virtual ITablePageQuery orderAsc(string column)
        {
            addOrder(column, "asc");
            return this;
        }

        public virtual ITablePageQuery orderDesc(string column)
        {
            addOrder(column, "desc");
            return this;
        }

        public virtual string TableName
        {
            get
            {
                return tableName_Renamed;
            }
        }

        protected internal virtual void addOrder(string column, string sortOrder)
        {
            if (string.ReferenceEquals(order, null))
            {
                order = "";
            }
            else
            {
                order = order + ", ";
            }
            order = order + column + " " + sortOrder;
        }

        public virtual TablePage listPage(int firstResult, int maxResults)
        {
            this.firstResult = firstResult;
            this.maxResults = maxResults;
            return commandExecutor.execute(this);
        }

        public virtual TablePage execute(ICommandContext commandContext)
        {
            return commandContext.TableDataManager.getTablePage(this, firstResult, maxResults);
        }

        public virtual string Order
        {
            get
            {
                return order;
            }
        }

    }

}