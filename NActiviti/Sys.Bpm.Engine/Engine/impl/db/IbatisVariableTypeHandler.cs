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

namespace Sys.Workflow.Engine.Impl.DB
{
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Persistence;
    using Sys.Workflow.Engine.Impl.Variable;
    using SmartSql.Abstractions.TypeHandler;
    using System;
    using System.Data;
    using System.Data.Common;

    /// <summary>
    /// 
    /// </summary>
    public class IbatisVariableTypeHandler : ITypeHandler<IVariableType>, ITypeHandler
    {
        /// <summary>
        /// 
        /// </summary>
        protected internal IVariableTypes variableTypes;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rs"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public virtual IVariableType GetResult(DbDataReader rs, string columnName)
        {
            return GetResult(rs, rs.GetOrdinal(columnName));
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal virtual IVariableTypes VariableTypes
        {
            get
            {
                if (variableTypes is null)
                {
                    variableTypes = Context.ProcessEngineConfiguration.VariableTypes;
                }
                return variableTypes;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rs"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public virtual IVariableType GetResult(DbDataReader rs, int columnIndex)
        {
            string typeName = rs.GetString(columnIndex);
            IVariableType type = VariableTypes.GetVariableType(typeName);
            if (type is null)
            {
                throw new ActivitiException("unknown variable type name " + typeName);
            }
            return type;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataReader"></param>
        /// <param name="columnName"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public virtual object GetValue(IDataReader dataReader, string columnName, Type targetType)
        {
            int ordinal = dataReader.GetOrdinal(columnName);
            return GetValue(dataReader, ordinal, targetType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataReader"></param>
        /// <param name="columnIndex"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public virtual object GetValue(IDataReader dataReader, int columnIndex, Type targetType)
        {
            try
            {
                string type = dataReader.GetString(columnIndex);
                if (string.IsNullOrWhiteSpace(type))
                {
                    return null;
                }

                return VariableTypes.GetVariableType(type);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataParameter"></param>
        /// <param name="parameterValue"></param>
        public virtual void SetParameter(IDataParameter dataParameter, object parameterValue)
        {
            dataParameter.Value = ToParameterValue(parameterValue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public object ToParameterValue(object value)
        {
            if (value is null)
            {
                return DBNull.Value;
            }

            if (value is IVariableType vt)
            {
                return vt.TypeName;
            }

            return value;
        }
    }
}