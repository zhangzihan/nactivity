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

namespace org.activiti.engine.impl.persistence.entity
{
    using DatabaseSchemaReader;
    using DatabaseSchemaReader.DataSchema;
    using org.activiti.engine.history;
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.db;
    using org.activiti.engine.management;
    using org.activiti.engine.repository;
    using org.activiti.engine.runtime;
    using org.activiti.engine.task;
    using Sys;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class TableDataManagerImpl : AbstractManager, ITableDataManager
    {

        public TableDataManagerImpl(ProcessEngineConfigurationImpl processEngineConfiguration) : base(processEngineConfiguration)
        {
        }

        public static IDictionary<Type, string> apiTypeToTableNameMap = new Dictionary<Type, string>();
        public static IDictionary<Type, string> entityToTableNameMap = new Dictionary<Type, string>();

        static TableDataManagerImpl()
        {
            // runtime
            entityToTableNameMap[typeof(ITaskEntity)] = "ACT_RU_TASK";
            entityToTableNameMap[typeof(IExecutionEntity)] = "ACT_RU_EXECUTION";
            entityToTableNameMap[typeof(IIdentityLinkEntity)] = "ACT_RU_IDENTITYLINK";
            entityToTableNameMap[typeof(IVariableInstanceEntity)] = "ACT_RU_VARIABLE";

            entityToTableNameMap[typeof(IJobEntity)] = "ACT_RU_JOB";
            entityToTableNameMap[typeof(ITimerJobEntity)] = "ACT_RU_TIMER_JOB";
            entityToTableNameMap[typeof(ISuspendedJobEntity)] = "ACT_RU_SUSPENDED_JOB";
            entityToTableNameMap[typeof(IDeadLetterJobEntity)] = "ACT_RU_DEADLETTER_JOB";


            entityToTableNameMap[typeof(IEventSubscriptionEntity)] = "ACT_RU_EVENT_SUBSCR";
            entityToTableNameMap[typeof(ICompensateEventSubscriptionEntity)] = "ACT_RU_EVENT_SUBSCR";
            entityToTableNameMap[typeof(IMessageEventSubscriptionEntity)] = "ACT_RU_EVENT_SUBSCR";
            entityToTableNameMap[typeof(ISignalEventSubscriptionEntity)] = "ACT_RU_EVENT_SUBSCR";

            // repository
            entityToTableNameMap[typeof(IDeploymentEntity)] = "ACT_RE_DEPLOYMENT";
            entityToTableNameMap[typeof(IProcessDefinitionEntity)] = "ACT_RE_PROCDEF";
            entityToTableNameMap[typeof(IModelEntity)] = "ACT_RE_MODEL";
            entityToTableNameMap[typeof(IProcessDefinitionInfoEntity)] = "ACT_PROCDEF_INFO";

            // history
            entityToTableNameMap[typeof(ICommentEntity)] = "ACT_HI_COMMENT";

            entityToTableNameMap[typeof(IHistoricActivityInstanceEntity)] = "ACT_HI_ACTINST";
            entityToTableNameMap[typeof(IAttachmentEntity)] = "ACT_HI_ATTACHMENT";
            entityToTableNameMap[typeof(IHistoricProcessInstanceEntity)] = "ACT_HI_PROCINST";
            entityToTableNameMap[typeof(IHistoricVariableInstanceEntity)] = "ACT_HI_VARINST";
            entityToTableNameMap[typeof(IHistoricTaskInstanceEntity)] = "ACT_HI_TASKINST";
            entityToTableNameMap[typeof(IHistoricIdentityLinkEntity)] = "ACT_HI_IDENTITYLINK";

            // a couple of stuff goes to the same table
            entityToTableNameMap[typeof(IHistoricDetailAssignmentEntity)] = "ACT_HI_DETAIL";
            entityToTableNameMap[typeof(IHistoricDetailTransitionInstanceEntity)] = "ACT_HI_DETAIL";
            entityToTableNameMap[typeof(IHistoricDetailVariableInstanceUpdateEntity)] = "ACT_HI_DETAIL";
            entityToTableNameMap[typeof(IHistoricDetailEntity)] = "ACT_HI_DETAIL";

            // general
            entityToTableNameMap[typeof(IPropertyEntity)] = "ACT_GE_PROPERTY";
            entityToTableNameMap[typeof(IByteArrayEntity)] = "ACT_GE_BYTEARRAY";
            entityToTableNameMap[typeof(IResourceEntity)] = "ACT_GE_BYTEARRAY";

            entityToTableNameMap[typeof(IEventLogEntryEntity)] = "ACT_EVT_LOG";

            // and now the map for the API types (does not cover all cases)
            apiTypeToTableNameMap[typeof(ITask)] = "ACT_RU_TASK";
            apiTypeToTableNameMap[typeof(IExecution)] = "ACT_RU_EXECUTION";
            apiTypeToTableNameMap[typeof(IProcessInstance)] = "ACT_RU_EXECUTION";
            apiTypeToTableNameMap[typeof(IProcessDefinition)] = "ACT_RE_PROCDEF";
            apiTypeToTableNameMap[typeof(IDeployment)] = "ACT_RE_DEPLOYMENT";
            apiTypeToTableNameMap[typeof(IJob)] = "ACT_RU_JOB";
            apiTypeToTableNameMap[typeof(IModel)] = "ACT_RE_MODEL";

            // history
            apiTypeToTableNameMap[typeof(IHistoricProcessInstance)] = "ACT_HI_PROCINST";
            apiTypeToTableNameMap[typeof(IHistoricActivityInstance)] = "ACT_HI_ACTINST";
            apiTypeToTableNameMap[typeof(IHistoricDetail)] = "ACT_HI_DETAIL";
            apiTypeToTableNameMap[typeof(IHistoricVariableUpdate)] = "ACT_HI_DETAIL";
            apiTypeToTableNameMap[typeof(IHistoricTaskInstance)] = "ACT_HI_TASKINST";
            apiTypeToTableNameMap[typeof(IHistoricVariableInstance)] = "ACT_HI_VARINST";


            // TODO: Identity skipped for the moment as no SQL injection is provided
            // here
        }

        protected internal virtual DbSqlSession DbSqlSession
        {
            get
            {
                return getSession<DbSqlSession>();
            }
        }

        public virtual IDictionary<string, long> TableCount
        {
            get
            {
                IDictionary<string, long> tableCount = new Dictionary<string, long>();
                try
                {
                    foreach (string tableName in TablesPresentInDatabase)
                    {
                        tableCount[tableName] = getTableCount(tableName);
                    }
                    //log.debug("Number of rows per activiti table: {}", tableCount);
                }
                catch (Exception e)
                {
                    throw new ActivitiException("couldn't get table counts", e);
                }
                return tableCount;
            }
        }

        public virtual IList<string> TablesPresentInDatabase
        {
            get
            {
                IList<string> tableNames = new List<string>();
                try
                {
                    var dbreader = ProcessEngineServiceProvider.Resolve<IDatabaseReader>();

                    //log.debug("retrieving activiti tables from jdbc metadata");
                    string databaseTablePrefix = DbSqlSession.DbSqlSessionFactory.DatabaseTablePrefix;
                    string tableNameFilter = databaseTablePrefix + "ACT_%";
                    if ("postgres".Equals(DbSqlSession.DbSqlSessionFactory.DatabaseType))
                    {
                        tableNameFilter = databaseTablePrefix + "act_%";
                    }
                    //if ("oracle".Equals(DbSqlSession.DbSqlSessionFactory.DatabaseType))
                    //{
                    //    tableNameFilter = databaseTablePrefix + "ACT" + databaseMetaData.SearchStringEscape + "_%";
                    //}

                    string catalog = null;
                    if (!ReferenceEquals(ProcessEngineConfiguration.DatabaseCatalog, null) && ProcessEngineConfiguration.DatabaseCatalog.Length > 0)
                    {
                        catalog = ProcessEngineConfiguration.DatabaseCatalog;
                    }

                    string schema = null;
                    if (!ReferenceEquals(ProcessEngineConfiguration.DatabaseSchema, null) && ProcessEngineConfiguration.DatabaseSchema.Length > 0)
                    {
                        if ("oracle".Equals(DbSqlSession.DbSqlSessionFactory.DatabaseType))
                        {
                            schema = ProcessEngineConfiguration.DatabaseSchema.ToUpper();
                        }
                        else
                        {
                            schema = ProcessEngineConfiguration.DatabaseSchema;
                        }
                    }

                    tableNames = dbreader.TableList().Where(x => new Regex(tableNameFilter, RegexOptions.IgnoreCase).IsMatch(x.Name)).Select(x => x.Name.ToUpper()).ToList();

                    //.getTables(catalog, schema, tableNameFilter, DbSqlSession.JDBC_METADATA_TABLE_TYPES);
                    //while (tables.next())
                    //{
                    //    string tableName = tables.getString("TABLE_NAME");
                    //    tableName = tableName.ToUpper();
                    //    tableNames.Add(tableName);
                    //    //log.debug("  retrieved activiti table name {}", tableName);
                    //}
                }
                catch (Exception e)
                {
                    throw new ActivitiException("couldn't get activiti table names using metadata: " + e.Message, e);
                }
                return tableNames;
            }
        }

        protected internal virtual long getTableCount(string tableName)
        {
            //log.debug("selecting table count for {}", tableName);
            long? count = (long?)DbSqlSession.selectOne<TableDataManagerImpl, long?>("selectTableCount", tableName);
            return count.Value;
        }

        public virtual TablePage getTablePage(TablePageQueryImpl tablePageQuery, int firstResult, int maxResults)
        {

            TablePage tablePage = new TablePage();

            IList<Dictionary<string, object>> tableData = DbSqlSession.selectList<PropertyEntityImpl, Dictionary<string, object>>("selectTableData", null, firstResult, maxResults);

            tablePage.TableName = tablePageQuery.TableName;
            tablePage.Total = getTableCount(tablePageQuery.TableName);
            tablePage.Rows = (IList<IDictionary<string, object>>)tableData;
            tablePage.FirstResult = firstResult;

            return tablePage;
        }

        public virtual string getTableName(Type entityClass, bool withPrefix)
        {
            string databaseTablePrefix = DbSqlSession.DbSqlSessionFactory.DatabaseTablePrefix;
            string tableName = null;

            if (entityClass.IsAssignableFrom(typeof(IEntity)))
            {
                tableName = entityToTableNameMap[entityClass];
            }
            else
            {
                tableName = apiTypeToTableNameMap[entityClass];
            }
            if (withPrefix)
            {
                return databaseTablePrefix + tableName;
            }
            else
            {
                return tableName;
            }
        }

        public virtual TableMetaData getTableMetaData(string tableName)
        {
            TableMetaData result = new TableMetaData();
            try
            {
                result.TableName = tableName;
                //DatabaseMetaData metaData = DbSqlSession.SqlSession.Connection.MetaData;

                if ("postgres".Equals(DbSqlSession.DbSqlSessionFactory.DatabaseType))
                {
                    tableName = tableName.ToLower();
                }

                string catalog = null;
                if (!ReferenceEquals(ProcessEngineConfiguration.DatabaseCatalog, null) && ProcessEngineConfiguration.DatabaseCatalog.Length > 0)
                {
                    catalog = ProcessEngineConfiguration.DatabaseCatalog;
                }

                string schema = null;
                if (!ReferenceEquals(ProcessEngineConfiguration.DatabaseSchema, null) && ProcessEngineConfiguration.DatabaseSchema.Length > 0)
                {
                    if ("oracle".Equals(DbSqlSession.DbSqlSessionFactory.DatabaseType))
                    {
                        schema = ProcessEngineConfiguration.DatabaseSchema.ToUpper();
                    }
                    else
                    {
                        schema = ProcessEngineConfiguration.DatabaseSchema;
                    }
                }

                var dbreader = ProcessEngineServiceProvider.Resolve<IDatabaseReader>();

                DatabaseTable table = dbreader.AllTables().FirstOrDefault(x => string.Compare(tableName, x.Name, true) == 0) ?? new DatabaseTable();

                foreach (var col in table.Columns)
                {
                    //bool wrongSchema = false;
                    //if (!string.ReferenceEquals(schema, null) && schema.Length > 0)
                    //{
                    //    string columnName = col.Name;
                    //    if ("TABLE_SCHEM".Equals(columnName, StringComparison.CurrentCultureIgnoreCase) || "TABLE_SCHEMA".Equals(columnName, StringComparison.CurrentCultureIgnoreCase))
                    //    {
                    //        if (!schema.Equals(resultSet.getString(resultSet.MetaData.getColumnName(i + 1)), StringComparison.CurrentCultureIgnoreCase))
                    //        {
                    //            wrongSchema = true;
                    //        }
                    //        break;
                    //    }
                    //}

                    //if (!wrongSchema)
                    //{
                    string name = col.Name.ToUpper();
                    string type = col.DbDataType.ToUpper();// resultSet.getString("TYPE_NAME").ToUpper();
                    result.addColumnMetaData(name, type);
                    //}
                }

            }
            catch (SqlException e)
            {
                throw new ActivitiException("Could not retrieve database metadata: " + e.Message);
            }

            if (result.ColumnNames.Count == 0)
            {
                // According to API, when a table doesn't exist, null should be returned
                result = null;
            }
            return result;
        }

    }

}