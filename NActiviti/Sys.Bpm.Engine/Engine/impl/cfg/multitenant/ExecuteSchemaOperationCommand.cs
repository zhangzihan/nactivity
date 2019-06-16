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
namespace Sys.Workflow.engine.impl.cfg.multitenant
{
    using Sys.Workflow.engine.impl.interceptor;


    /// <summary>
    /// <seealso cref="Command"/> that is used by the <seealso cref="MultiSchemaMultiTenantProcessEngineConfiguration"/> to 
    /// make sure the 'databaseSchemaUpdate' setting is applied for each tenant datasource.
    /// 
    /// 
    /// </summary>
    public class ExecuteSchemaOperationCommand : ICommand<object>
    {

        protected internal string schemaOperation;

        protected internal ITenantInfoHolder tenantInfoHolder;

        public ExecuteSchemaOperationCommand(string schemaOperation)
        {
            this.schemaOperation = schemaOperation;
        }

        public virtual object Execute(ICommandContext commandContext)
        {
            if (ProcessEngineConfiguration.DB_SCHEMA_UPDATE_DROP_CREATE.Equals(schemaOperation))
            {
                try
                {
                    commandContext.DbSqlSession.DbSchemaDrop();
                }
                catch (Exception)
                {
                    // ignore
                }
            }
            if (ProcessEngineConfiguration.DB_SCHEMA_UPDATE_CREATE_DROP.Equals(schemaOperation) || ProcessEngineConfiguration.DB_SCHEMA_UPDATE_DROP_CREATE.Equals(schemaOperation) || ProcessEngineConfiguration.DB_SCHEMA_UPDATE_CREATE.Equals(schemaOperation))
            {
                commandContext.DbSqlSession.DbSchemaCreate();

            }
            else if (ProcessEngineConfiguration.DB_SCHEMA_UPDATE_FALSE.Equals(schemaOperation))
            {
                commandContext.DbSqlSession.DbSchemaCheckVersion();

            }
            else if (ProcessEngineConfiguration.DB_SCHEMA_UPDATE_TRUE.Equals(schemaOperation))
            {
                commandContext.DbSqlSession.DbSchemaUpdate();
            }

            return commandContext.GetResult();
        }
    }

}