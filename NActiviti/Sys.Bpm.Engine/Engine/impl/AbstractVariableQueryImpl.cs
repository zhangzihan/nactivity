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

namespace Sys.Workflow.Engine.Impl
{

    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Variable;

    /// <summary>
    /// Abstract query class that adds methods to query for variable values.
    /// 
    /// 
    /// </summary>
    public abstract class AbstractVariableQueryImpl<T, U> : AbstractQuery<T, U> where T : class
    {

        private const long serialVersionUID = 1L;

        protected internal IList<QueryVariableValue> queryVariableValues = new List<QueryVariableValue>();

        public AbstractVariableQueryImpl()
        {
        }

        public AbstractVariableQueryImpl(ICommandContext commandContext) : base(commandContext)
        {
        }

        public AbstractVariableQueryImpl(ICommandExecutor commandExecutor) : base(commandExecutor)
        {
        }

        public override abstract long ExecuteCount(ICommandContext commandContext);

        public override abstract IList<U> ExecuteList(ICommandContext commandContext, Page page);

        public virtual T VariableValueEquals(string name, object value)
        {
            return VariableValueEquals(name, value, true);
        }

        protected internal virtual T VariableValueEquals(string name, object value, bool localScope)
        {
            AddVariable(name, value, QueryOperator.EQUALS, localScope);

            return this as T;
        }

        public virtual T VariableValueEquals(object value)
        {
            return VariableValueEquals(value, true);
        }

        protected internal virtual T VariableValueEquals(object value, bool localScope)
        {
            queryVariableValues.Add(new QueryVariableValue(null, value, QueryOperator.EQUALS, localScope));

            return this as T;
        }

        public virtual T VariableValueEqualsIgnoreCase(string name, string value)
        {
            return VariableValueEqualsIgnoreCase(name, value, true);
        }

        protected internal virtual T VariableValueEqualsIgnoreCase(string name, string value, bool localScope)
        {
            if (value is null)
            {
                throw new ActivitiIllegalArgumentException("value is null");
            }
            AddVariable(name, value.ToLower(), QueryOperator.EQUALS_IGNORE_CASE, localScope);

            return this as T;
        }

        public virtual T VariableValueNotEqualsIgnoreCase(string name, string value)
        {
            return VariableValueNotEqualsIgnoreCase(name, value, true);
        }

        protected internal virtual T VariableValueNotEqualsIgnoreCase(string name, string value, bool localScope)
        {
            if (value is null)
            {
                throw new ActivitiIllegalArgumentException("value is null");
            }
            AddVariable(name, value.ToLower(), QueryOperator.NOT_EQUALS_IGNORE_CASE, localScope);

            return this as T;
        }

        public virtual T VariableValueNotEquals(string name, object value)
        {
            return VariableValueNotEquals(name, value, true);
        }

        protected internal virtual T VariableValueNotEquals(string name, object value, bool localScope)
        {
            AddVariable(name, value, QueryOperator.NOT_EQUALS, localScope);

            return this as T;
        }

        public virtual T VariableValueGreaterThan(string name, object value)
        {
            return VariableValueGreaterThan(name, value, true);
        }

        protected internal virtual T VariableValueGreaterThan(string name, object value, bool localScope)
        {
            AddVariable(name, value, QueryOperator.GREATER_THAN, localScope);

            return this as T;
        }

        public virtual T VariableValueGreaterThanOrEqual(string name, object value)
        {
            return VariableValueGreaterThanOrEqual(name, value, true);
        }

        protected internal virtual T VariableValueGreaterThanOrEqual(string name, object value, bool localScope)
        {
            AddVariable(name, value, QueryOperator.GREATER_THAN_OR_EQUAL, localScope);

            return this as T;
        }

        public virtual T VariableValueLessThan(string name, object value)
        {
            return VariableValueLessThan(name, value, true);
        }

        protected internal virtual T VariableValueLessThan(string name, object value, bool localScope)
        {
            AddVariable(name, value, QueryOperator.LESS_THAN, localScope);

            return this as T;
        }

        public virtual T VariableValueLessThanOrEqual(string name, object value)
        {
            return VariableValueLessThanOrEqual(name, value, true);
        }

        protected internal virtual T VariableValueLessThanOrEqual(string name, object value, bool localScope)
        {
            AddVariable(name, value, QueryOperator.LESS_THAN_OR_EQUAL, localScope);

            return this as T;
        }

        public virtual T VariableValueLike(string name, string value)
        {
            return VariableValueLike(name, value, true);
        }

        public virtual T VariableValueLikeIgnoreCase(string name, string value)
        {
            return VariableValueLikeIgnoreCase(name, value, true);
        }

        protected internal virtual T VariableValueLike(string name, string value, bool localScope)
        {
            AddVariable(name, value, QueryOperator.LIKE, localScope);

            return this as T;
        }

        protected internal virtual T VariableValueLikeIgnoreCase(string name, string value, bool localScope)
        {
            AddVariable(name, value.ToLower(), QueryOperator.LIKE_IGNORE_CASE, localScope);

            return this as T;
        }

        protected internal virtual void AddVariable(string name, object value, QueryOperator @operator, bool localScope)
        {
            if (name is null)
            {
                throw new ActivitiIllegalArgumentException("name is null");
            }
            if (value is null || IsBoolean(value))
            {
                // Null-values and booleans can only be used in EQUALS and
                // NOT_EQUALS
                switch (@operator)
                {
                    case QueryOperator.GREATER_THAN:
                        throw new ActivitiIllegalArgumentException("Booleans and null cannot be used in 'greater than' condition");
                    case QueryOperator.LESS_THAN:
                        throw new ActivitiIllegalArgumentException("Booleans and null cannot be used in 'less than' condition");
                    case QueryOperator.GREATER_THAN_OR_EQUAL:
                        throw new ActivitiIllegalArgumentException("Booleans and null cannot be used in 'greater than or equal' condition");
                    case QueryOperator.LESS_THAN_OR_EQUAL:
                        throw new ActivitiIllegalArgumentException("Booleans and null cannot be used in 'less than or equal' condition");
                }

                if (@operator == QueryOperator.EQUALS_IGNORE_CASE && !(value is string))
                {
                    throw new ActivitiIllegalArgumentException("Only string values can be used with 'equals ignore case' condition");
                }

                if (@operator == QueryOperator.NOT_EQUALS_IGNORE_CASE && !(value is string))
                {
                    throw new ActivitiIllegalArgumentException("Only string values can be used with 'not equals ignore case' condition");
                }

                if ((@operator == QueryOperator.LIKE || @operator == QueryOperator.LIKE_IGNORE_CASE) && !(value is string))
                {
                    throw new ActivitiIllegalArgumentException("Only string values can be used with 'like' condition");
                }
            }
            queryVariableValues.Add(new QueryVariableValue(name, value, @operator, localScope));
        }

        protected internal virtual bool IsBoolean(object value)
        {
            if (value is null)
            {
                return false;
            }
            return value.GetType().IsAssignableFrom(typeof(bool)) || value.GetType().IsAssignableFrom(typeof(bool));
        }

        protected internal virtual void EnsureVariablesInitialized()
        {
            if (queryVariableValues.Count > 0)
            {
                IVariableTypes variableTypes = Context.ProcessEngineConfiguration.VariableTypes;
                foreach (QueryVariableValue queryVariableValue in queryVariableValues)
                {
                    queryVariableValue.Initialize(variableTypes);
                }
            }
        }

        public virtual IList<QueryVariableValue> QueryVariableValues
        {
            get
            {
                return queryVariableValues;
            }
        }

        public virtual bool HasLocalQueryVariableValue()
        {
            foreach (QueryVariableValue qvv in queryVariableValues)
            {
                if (qvv.Local)
                {
                    return true;
                }
            }
            return false;
        }

        public virtual bool HasNonLocalQueryVariableValue()
        {
            foreach (QueryVariableValue qvv in queryVariableValues)
            {
                if (!qvv.Local)
                {
                    return true;
                }
            }
            return false;
        }

    }

}