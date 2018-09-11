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

    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.query;
    using Sys.Bpm;

    /// <summary>
    /// Abstract superclass for all native query types.
    /// 
    /// 
    /// </summary>
    [Serializable]
    public abstract class AbstractNativeQuery<T, U> : ICommand<object>, INativeQuery<T, U> where U : class
    {

        private const long serialVersionUID = 1L;

        [NonSerialized]
        protected internal ICommandExecutor commandExecutor;
        [NonSerialized]
        protected internal ICommandContext commandContext;

        protected internal int maxResults = int.MaxValue;
        protected internal int firstResult;
        protected internal ResultType? resultType;

        private IDictionary<string, object> parameters = new Dictionary<string, object>();
        private string sqlStatement;

        protected internal AbstractNativeQuery(ICommandExecutor commandExecutor)
        {
            this.commandExecutor = commandExecutor;
        }

        public AbstractNativeQuery(ICommandContext commandContext)
        {
            this.commandContext = commandContext;
        }

        public virtual AbstractNativeQuery<T, U> setCommandExecutor(ICommandExecutor commandExecutor)
        {
            this.commandExecutor = commandExecutor;
            return this;
        }

        public virtual T sql(string sqlStatement)
        {
            this.sqlStatement = sqlStatement;
            //return  this as T;
            return default(T);
        }

        public virtual T parameter(string name, object value)
        {
            parameters[name] = value;
            return default(T);
            //return (T)this;
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
            return executeList(Context.CommandContext, ParameterMap, 0, int.MaxValue);
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
            return executeList(Context.CommandContext, ParameterMap, firstResult, maxResults);
        }

        public virtual long count()
        {
            this.resultType = ResultType.COUNT;
            if (commandExecutor != null)
            {
                return ((long?)commandExecutor.execute(this)).GetValueOrDefault();
            }
            return executeCount(Context.CommandContext, ParameterMap);
        }

        public virtual object execute(ICommandContext commandContext)
        {
            if (resultType == ResultType.LIST)
            {
                return executeList(commandContext, ParameterMap, 0, int.MaxValue);
            }
            else if (resultType == ResultType.LIST_PAGE)
            {
                IDictionary<string, object> parameterMap = ParameterMap;
                parameterMap["resultType"] = "LIST_PAGE";
                parameterMap["firstResult"] = firstResult;
                parameterMap["maxResults"] = maxResults;
                parameterMap.TryGetValue("orderBy", out var orderBy);
                if (!string.IsNullOrWhiteSpace(orderBy?.ToString()))
                {
                    parameterMap["orderByColumns"] = "RES." + orderBy;
                }
                else
                {
                    parameterMap["orderByColumns"] = "RES.ID_ asc";
                }

                int firstRow = firstResult + 1;
                parameterMap["firstRow"] = firstRow;
                int lastRow = 0;
                if (maxResults == int.MaxValue)
                {
                    lastRow = maxResults;
                }
                else
                {
                    lastRow = firstResult + maxResults + 1;
                }
                parameterMap["lastRow"] = lastRow;
                return executeList(commandContext, parameterMap, firstResult, maxResults);
            }
            else if (resultType == ResultType.SINGLE_RESULT)
            {
                return executeSingleResult(commandContext);
            }
            else
            {
                return executeCount(commandContext, ParameterMap);
            }
        }

        public abstract long executeCount(ICommandContext commandContext, IDictionary<string, object> parameterMap);

        /// <summary>
        /// Executes the actual query to retrieve the list of results.
        /// </summary>
        /// <param name="maxResults"> </param>
        /// <param name="firstResult">
        /// </param>
        /// <param name="page">
        ///          used if the results must be paged. If null, no paging will be applied. </param>
        public abstract IList<U> executeList(ICommandContext commandContext, IDictionary<string, object> parameterMap, int firstResult, int maxResults);

        public virtual U executeSingleResult(ICommandContext commandContext)
        {
            IList<U> results = executeList(commandContext, ParameterMap, 0, int.MaxValue);
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

        private IDictionary<string, object> ParameterMap
        {
            get
            {
                Dictionary<string, object> parameterMap = new Dictionary<string, object>();
                parameterMap["sql"] = sqlStatement;
                parameterMap.putAll(parameters);
                return parameterMap;
            }
        }

        public virtual IDictionary<string, object> Parameters
        {
            get
            {
                return parameters;
            }
        }

    }

}