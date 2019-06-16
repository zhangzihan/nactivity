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
namespace Sys.Workflow.Engine.Impl
{

    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Management;

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

        public virtual ITablePageQuery SetTableName(string tableName)
        {
            this.tableName_Renamed = tableName;
            return this;
        }

        public virtual ITablePageQuery OrderAsc(string column)
        {
            AddOrder(column, "asc");
            return this;
        }

        public virtual ITablePageQuery OrderDesc(string column)
        {
            AddOrder(column, "desc");
            return this;
        }

        public virtual string TableName
        {
            get
            {
                return tableName_Renamed;
            }
        }

        protected internal virtual void AddOrder(string column, string sortOrder)
        {
            if (order is null)
            {
                order = "";
            }
            else
            {
                order += ", ";
            }
            order = $"{order}{column} {sortOrder}";
        }

        public virtual TablePage ListPage(int firstResult, int maxResults)
        {
            this.firstResult = firstResult;
            this.maxResults = maxResults;
            return commandExecutor.Execute(this);
        }

        public virtual TablePage Execute(ICommandContext commandContext)
        {
            return commandContext.TableDataManager.GetTablePage(this, firstResult, maxResults);
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