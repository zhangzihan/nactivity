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
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Impl.Variable;

    /// <summary>
    /// Represents a variable value used in queries.
    /// 
    /// 
    /// </summary>
    [Serializable]
    public class QueryVariableValue
    {
        private const long serialVersionUID = 1L;
        private readonly string name;
        private readonly object value;
        private readonly QueryOperator? @operator;

        private IVariableInstanceEntity variableInstanceEntity;
        private readonly bool local;

        public QueryVariableValue(string name, object value, QueryOperator @operator, bool local)
        {
            this.name = name;
            this.value = value;
            this.@operator = @operator;
            this.local = local;
        }

        public virtual void Initialize(IVariableTypes types)
        {
            if (variableInstanceEntity == null)
            {
                IVariableType type = types.FindVariableType(value);
                if (type is ByteArrayType)
                {
                    throw new ActivitiIllegalArgumentException("Variables of type ByteArray cannot be used to query");
                }
                else if (type is IJPAEntityVariableType && @operator != QueryOperator.EQUALS)
                {
                    throw new ActivitiIllegalArgumentException("JPA entity variables can only be used in 'variableValueEquals'");
                }
                else if (type is IJPAEntityListVariableType)
                {
                    throw new ActivitiIllegalArgumentException("Variables containing a list of JPA entities cannot be used to query");
                }
                else
                {
                    // Type implementation determines which fields are set on the entity
                    variableInstanceEntity = Context.CommandContext.VariableInstanceEntityManager.Create(name, type, value);
                }
            }
        }

        public virtual string Name
        {
            get
            {
                return name;
            }
        }

        public virtual string Operator
        {
            get
            {
                if (@operator != null)
                {
                    return @operator.ToString();
                }

                return QueryOperator.EQUALS.ToString();
            }
        }

        public virtual string TextValue
        {
            get
            {
                if (variableInstanceEntity != null)
                {
                    return variableInstanceEntity.TextValue;
                }
                return null;
            }
        }

        public virtual long? LongValue
        {
            get
            {
                if (variableInstanceEntity != null)
                {
                    return variableInstanceEntity.LongValue;
                }
                return null;
            }
        }

        public virtual double? DoubleValue
        {
            get
            {
                if (variableInstanceEntity != null)
                {
                    return variableInstanceEntity.DoubleValue;
                }
                return null;
            }
        }

        public virtual string TextValue2
        {
            get
            {
                if (variableInstanceEntity != null)
                {
                    return variableInstanceEntity.TextValue2;
                }
                return null;
            }
        }

        public virtual string Type
        {
            get
            {
                if (variableInstanceEntity != null)
                {
                    return variableInstanceEntity.Type.TypeName;
                }
                return null;
            }
        }

        public virtual bool Local
        {
            get
            {
                return local;
            }
        }
    }
}