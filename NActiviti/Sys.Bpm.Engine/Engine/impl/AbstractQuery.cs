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
        protected internal string orderBy_Renamed;

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

        public virtual AbstractQuery<T, U> setCommandExecutor(ICommandExecutor commandExecutor)
        {
            this.commandExecutor = commandExecutor;

            return this;
        }

        public virtual T orderBy(IQueryProperty property)
        {
            this.orderProperty = property;

            return this as T;
        }

        public virtual T orderBy(IQueryProperty property, NullHandlingOnOrder nullHandlingOnOrder)
        {
            orderBy(property);
            this.nullHandlingOnOrder = nullHandlingOnOrder;

            return this as T;
        }

        public virtual T asc()
        {
            return direction(Direction.ASCENDING);
        }

        public virtual T desc()
        {
            return direction(Direction.DESCENDING);
        }

        public virtual T direction(Direction direction)
        {
            if (orderProperty == null)
            {
                throw new ActivitiIllegalArgumentException("You should call any of the orderBy methods first before specifying a direction");
            }

            addOrder(orderProperty.Name, direction.Name, nullHandlingOnOrder);
            orderProperty = null;

            return this as T;
        }

        protected internal virtual void checkQueryOk()
        {
            if (orderProperty != null)
            {
                throw new ActivitiIllegalArgumentException("Invalid query: call asc() or desc() after using orderByXX()");
            }
        }

        public virtual U singleResult()
        {
            this.resultType = ResultType.SINGLE_RESULT;
            if (commandExecutor != null)
            {
                return (U)commandExecutor.execute(this);
            }
            return executeSingleResult(Context.CommandContext);
        }

        public virtual IList<U> list()
        {
            this.resultType = ResultType.LIST;
            if (commandExecutor != null)
            {
                return (IList<U>)commandExecutor.execute(this);
            }
            return executeList(Context.CommandContext, null);
        }

        public virtual IList<U> listPage(int firstResult, int maxResults)
        {
            this.firstResult = firstResult;
            this.maxResults = maxResults;
            this.resultType = ResultType.LIST_PAGE;
            if (commandExecutor != null)
            {
                return (IList<U>)commandExecutor.execute(this);
            }
            return executeList(Context.CommandContext, new Page(firstResult, maxResults));
        }

        public virtual long count()
        {
            this.resultType = ResultType.COUNT;
            if (commandExecutor != null)
            {
                return (long)commandExecutor.execute(this);
            }
            return executeCount(Context.CommandContext);
        }

        public virtual object execute(ICommandContext commandContext)
        {
            if (resultType == ResultType.LIST)
            {
                return executeList(commandContext, null);
            }
            else if (resultType == ResultType.SINGLE_RESULT)
            {
                return executeSingleResult(commandContext);
            }
            else if (resultType == ResultType.LIST_PAGE)
            {
                return executeList(commandContext, null);
            }
            else
            {
                return executeCount(commandContext);
            }
        }

        public abstract long executeCount(ICommandContext commandContext);

        /// <summary>
        /// Executes the actual query to retrieve the list of results.
        /// </summary>
        /// <param name="page">
        ///          used if the results must be paged. If null, no paging will be applied. </param>
        public abstract IList<U> executeList(ICommandContext commandContext, Page page);

        public virtual U executeSingleResult(ICommandContext commandContext)
        {
            IList<U> results = executeList(commandContext, null);
            if (results.Count == 1)
            {
                return results[0];
            }
            else if (results.Count > 1)
            {
                throw new ActivitiException("Query return " + results.Count + " results instead of max 1");
            }
            return default(U);
        }

        protected internal virtual void addOrder(string column, string sortOrder, NullHandlingOnOrder? nullHandlingOnOrder)
        {

            if (string.ReferenceEquals(orderBy_Renamed, null))
            {
                orderBy_Renamed = "";
            }
            else
            {
                orderBy_Renamed = orderBy_Renamed + ", ";
            }

            string defaultOrderByClause = column + " " + sortOrder;

            if (nullHandlingOnOrder.HasValue)
            {

                if (nullHandlingOnOrder.Value == NullHandlingOnOrder.NULLS_FIRST)
                {

                    if (ProcessEngineConfigurationImpl.DATABASE_TYPE_H2.Equals(databaseType) || ProcessEngineConfigurationImpl.DATABASE_TYPE_HSQL.Equals(databaseType) || ProcessEngineConfigurationImpl.DATABASE_TYPE_POSTGRES.Equals(databaseType) || ProcessEngineConfigurationImpl.DATABASE_TYPE_ORACLE.Equals(databaseType))
                    {
                        orderBy_Renamed = orderBy_Renamed + defaultOrderByClause + " NULLS FIRST";
                    }
                    else if (ProcessEngineConfigurationImpl.DATABASE_TYPE_MYSQL.Equals(databaseType))
                    {
                        orderBy_Renamed = orderBy_Renamed + "isnull(" + column + ") desc," + defaultOrderByClause;
                    }
                    else
                    {
                        orderBy_Renamed = orderBy_Renamed + defaultOrderByClause;
                    }

                }
                else if (nullHandlingOnOrder.Value == NullHandlingOnOrder.NULLS_LAST)
                {

                    if (ProcessEngineConfigurationImpl.DATABASE_TYPE_H2.Equals(databaseType) || ProcessEngineConfigurationImpl.DATABASE_TYPE_HSQL.Equals(databaseType) || ProcessEngineConfigurationImpl.DATABASE_TYPE_POSTGRES.Equals(databaseType) || ProcessEngineConfigurationImpl.DATABASE_TYPE_ORACLE.Equals(databaseType))
                    {
                        orderBy_Renamed = orderBy_Renamed + column + " " + sortOrder + " NULLS LAST";
                    }
                    else if (ProcessEngineConfigurationImpl.DATABASE_TYPE_MYSQL.Equals(databaseType))
                    {
                        orderBy_Renamed = orderBy_Renamed + "isnull(" + column + ") asc," + defaultOrderByClause;
                    }
                    else
                    {
                        orderBy_Renamed = orderBy_Renamed + defaultOrderByClause;
                    }

                }

            }
            else
            {
                orderBy_Renamed = orderBy_Renamed + defaultOrderByClause;
            }

        }

        public override string OrderBy
        {
            get
            {
                if (string.ReferenceEquals(orderBy_Renamed, null))
                {
                    return base.OrderBy;
                }
                else
                {
                    return orderBy_Renamed;
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