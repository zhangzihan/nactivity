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
namespace org.activiti.engine.impl
{

    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.db;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.query;


    public enum ResultType
    {
        LIST,
        LIST_PAGE,
        SINGLE_RESULT,
        COUNT
    }

    /// <summary>
    /// Abstract superclass for all query types.
    /// 
    /// 
    /// </summary>
    [Serializable]
    public abstract class AbstractQuery<T, U> : ListQueryParameterObject, ICommand<object>, IQuery<T, U> where T : class
    {

        private const long serialVersionUID = 1L;

        public static readonly string SORTORDER_ASC = "asc";
        public static readonly string SORTORDER_DESC = "desc";

        [NonSerialized]
        protected internal ICommandExecutor commandExecutor;
        [NonSerialized]
        protected internal ICommandContext commandContext;

        protected internal new string databaseType;
        protected internal string _orderBy;

        protected internal ResultType? resultType;

        protected internal IQueryProperty orderProperty;

        public enum NullHandlingOnOrder
        {
            NULLS_FIRST,
            NULLS_LAST
        }

        protected internal NullHandlingOnOrder? nullHandlingOnOrder;

        protected internal AbstractQuery()
        {
            parameter = this;
        }

        protected internal AbstractQuery(ICommandExecutor commandExecutor)
        {
            this.commandExecutor = commandExecutor;
        }

        public AbstractQuery(ICommandContext commandContext)
        {
            this.commandContext = commandContext;
        }

        // To be used by custom queries
        public AbstractQuery(IManagementService managementService) : this(((ManagementServiceImpl)managementService).CommandExecutor)
        {
        }

        public virtual AbstractQuery<T, U> SetCommandExecutor(ICommandExecutor commandExecutor)
        {
            this.commandExecutor = commandExecutor;

            return this;
        }

        public virtual T SetOrderBy(IQueryProperty property)
        {
            this.orderProperty = property;

            return this as T;
        }

        public virtual T SetOrderBy(IQueryProperty property, NullHandlingOnOrder nullHandlingOnOrder)
        {
            SetOrderBy(property);
            this.nullHandlingOnOrder = nullHandlingOnOrder;

            return this as T;
        }

        public virtual T Asc()
        {
            return Direction(impl.Direction.ASCENDING);
        }

        public virtual T Desc()
        {
            return Direction(impl.Direction.DESCENDING);
        }

        public virtual T Direction(Direction direction)
        {
            if (orderProperty == null)
            {
                throw new ActivitiIllegalArgumentException("You should call any of the orderBy methods first before specifying a direction");
            }

            AddOrder(orderProperty.Name, direction.Name, nullHandlingOnOrder);
            orderProperty = null;

            return this as T;
        }

        protected internal virtual void CheckQueryOk()
        {
            if (orderProperty != null)
            {
                throw new ActivitiIllegalArgumentException("Invalid query: call asc() or desc() after using orderByXX()");
            }
        }

        public virtual U SingleResult()
        {
            this.resultType = ResultType.SINGLE_RESULT;
            if (commandExecutor != null)
            {
                return (U)commandExecutor.Execute(this);
            }
            return ExecuteSingleResult(Context.CommandContext);
        }

        public virtual IList<U> List()
        {
            this.resultType = ResultType.LIST;
            if (commandExecutor != null)
            {
                return (IList<U>)commandExecutor.Execute(this);
            }
            return ExecuteList(Context.CommandContext, null);
        }

        public virtual IList<U> ListPage(int firstResult, int maxResults)
        {
            this.firstResult = firstResult;
            this.maxResults = maxResults;
            this.resultType = ResultType.LIST_PAGE;
            if (commandExecutor != null)
            {
                return (IList<U>)commandExecutor.Execute(this);
            }
            return ExecuteList(Context.CommandContext, new Page(firstResult, maxResults));
        }

        public virtual long Count()
        {
            this.resultType = ResultType.COUNT;
            if (commandExecutor != null)
            {
                return (long)commandExecutor.Execute(this);
            }
            return ExecuteCount(Context.CommandContext);
        }

        public virtual object Execute(ICommandContext commandContext)
        {
            if (resultType == ResultType.LIST)
            {
                return ExecuteList(commandContext, null);
            }
            else if (resultType == ResultType.SINGLE_RESULT)
            {
                return ExecuteSingleResult(commandContext);
            }
            else if (resultType == ResultType.LIST_PAGE)
            {
                return ExecuteList(commandContext, null);
            }
            else
            {
                return ExecuteCount(commandContext);
            }
        }

        public abstract long ExecuteCount(ICommandContext commandContext);

        /// <summary>
        /// Executes the actual query to retrieve the list of results.
        /// </summary>
        /// <param name="page">
        ///          used if the results must be paged. If null, no paging will be applied. </param>
        public abstract IList<U> ExecuteList(ICommandContext commandContext, Page page);

        public virtual U ExecuteSingleResult(ICommandContext commandContext)
        {
            IList<U> results = ExecuteList(commandContext, null);
            if (results.Count == 1)
            {
                return results[0];
            }
            else if (results.Count > 1)
            {
                throw new ActivitiException("Query return " + results.Count + " results instead of max 1");
            }
            return default;
        }

        protected internal virtual void AddOrder(string column, string sortOrder, NullHandlingOnOrder? nullHandlingOnOrder)
        {

            if (_orderBy is null)
            {
                _orderBy = "";
            }
            else
            {
                _orderBy += ", ";
            }

            string defaultOrderByClause = column + " " + sortOrder;

            if (nullHandlingOnOrder.HasValue)
            {

                if (nullHandlingOnOrder.Value == NullHandlingOnOrder.NULLS_FIRST)
                {

                    if (ProcessEngineConfigurationImpl.DATABASE_TYPE_H2.Equals(databaseType) || ProcessEngineConfigurationImpl.DATABASE_TYPE_HSQL.Equals(databaseType) || ProcessEngineConfigurationImpl.DATABASE_TYPE_POSTGRES.Equals(databaseType) || ProcessEngineConfigurationImpl.DATABASE_TYPE_ORACLE.Equals(databaseType))
                    {
                        _orderBy = _orderBy + defaultOrderByClause + " NULLS FIRST";
                    }
                    else if (ProcessEngineConfigurationImpl.DATABASE_TYPE_MYSQL.Equals(databaseType))
                    {
                        _orderBy = _orderBy + "isnull(" + column + ") desc," + defaultOrderByClause;
                    }
                    else
                    {
                        _orderBy += defaultOrderByClause;
                    }

                }
                else if (nullHandlingOnOrder.Value == NullHandlingOnOrder.NULLS_LAST)
                {

                    if (ProcessEngineConfigurationImpl.DATABASE_TYPE_H2.Equals(databaseType) || ProcessEngineConfigurationImpl.DATABASE_TYPE_HSQL.Equals(databaseType) || ProcessEngineConfigurationImpl.DATABASE_TYPE_POSTGRES.Equals(databaseType) || ProcessEngineConfigurationImpl.DATABASE_TYPE_ORACLE.Equals(databaseType))
                    {
                        _orderBy = _orderBy + column + " " + sortOrder + " NULLS LAST";
                    }
                    else if (ProcessEngineConfigurationImpl.DATABASE_TYPE_MYSQL.Equals(databaseType))
                    {
                        _orderBy = _orderBy + "isnull(" + column + ") asc," + defaultOrderByClause;
                    }
                    else
                    {
                        _orderBy += defaultOrderByClause;
                    }

                }

            }
            else
            {
                _orderBy += defaultOrderByClause;
            }

        }

        public override string OrderBy
        {
            get
            {
                if (_orderBy is null)
                {
                    return base.OrderBy;
                }
                else
                {
                    return _orderBy;
                }
            }
        }

        public override string OrderByColumns
        {
            get
            {
                return OrderBy;
            }
        }

        public override string DatabaseType
        {
            get
            {
                return databaseType;
            }
            set
            {
                this.databaseType = value;
            }
        }
    }
}