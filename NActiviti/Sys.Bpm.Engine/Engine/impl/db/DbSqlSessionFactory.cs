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

namespace Sys.Workflow.Engine.Impl.DB
{
    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using SmartSql.Abstractions;

    /// 
    /// 
    public class DbSqlSessionFactory : ISessionFactory
    {
        /// <summary>
        /// 
        /// </summary>
        protected internal static readonly IDictionary<string, IDictionary<string, string>> databaseSpecificStatements = new Dictionary<string, IDictionary<string, string>>();

        /// <summary>
        /// A map {class, boolean}, to indicate whether or not a certain <seealso cref="IEntity"/> class can be bulk inserted.
        /// </summary>
        protected internal static IDictionary<Type, bool> bulkInsertableMap;

        /// <summary>
        /// 
        /// </summary>
        protected internal string databaseType;

        /// <summary>
        /// 
        /// </summary>
        protected internal string databaseTablePrefix = "";

        /// <summary>
        /// 
        /// </summary>
        private bool tablePrefixIsSchema;

        /// <summary>
        /// 
        /// </summary>
        protected internal string databaseCatalog;

        /// <summary>
        /// In some situations you want to set the schema to use for table checks /
        /// generation if the database metadata doesn't return that correctly, see
        /// https://activiti.atlassian.net/browse/ACT-1220,
        /// https://activiti.atlassian.net/browse/ACT-1062
        /// </summary>
        protected internal string databaseSchema;

        /// <summary>
        /// 
        /// </summary>
        protected internal IIdGenerator idGenerator;

        // Caches, filled while executing processes 
        /// <summary>
        /// 
        /// </summary>
        protected internal bool isDbHistoryUsed = true;

        /// <summary>
        /// 
        /// </summary>
        protected internal int maxNrOfStatementsInBulkInsert = 100;

        /// <summary>
        /// 
        /// </summary>
        public virtual Type SessionType
        {
            get
            {
                return typeof(DbSqlSession);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandContext"></param>
        /// <returns></returns>
        public virtual ISession OpenSession(ICommandContext commandContext)
        {
            DbSqlSession dbSqlSession = new DbSqlSession(this, commandContext.EntityCache);

            return dbSqlSession;
        }

        // insert, update and delete statements
        // /////////////////////////////////////

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clazz"></param>
        /// <param name="managedType"></param>
        /// <returns></returns>
        public virtual string GetInsertStatement(Type clazz, ref Type managedType)
        {
            return GetStatement(clazz, "insert", ref managedType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clazz"></param>
        /// <param name="managedType"></param>
        /// <returns></returns>
        public virtual string GetBulkInsertStatement(Type clazz, ref Type managedType)
        {
            return GetStatement(clazz, "bulkInsert", ref managedType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clazz"></param>
        /// <param name="managedType"></param>
        /// <returns></returns>
        public virtual string GetUpdateStatement(Type clazz, ref Type managedType)
        {
            return GetStatement(clazz, "update", ref managedType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clazz"></param>
        /// <param name="managedType"></param>
        /// <returns></returns>
        public virtual string GetDeleteStatement(Type clazz, ref Type managedType)
        {
            return GetStatement(clazz, "delete", ref managedType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clazz"></param>
        /// <param name="managedType"></param>
        /// <returns></returns>
        public virtual string GetBulkDeleteStatement(Type clazz, ref Type managedType)
        {
            return GetStatement(clazz, "bulkDelete", ref managedType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityClass"></param>
        /// <returns></returns>
        public virtual string GetSelectStatement(ref Type entityClass)
        {
            return GetStatement(entityClass, "select", ref entityClass);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clazz"></param>
        /// <param name="prefix"></param>
        /// <param name="managedType"></param>
        /// <returns></returns>
        private string GetStatement(Type clazz, string prefix, ref Type managedType)
        {
            if (clazz == typeof(HistoricDetailVariableInstanceUpdateEntityImpl))
            {
                managedType = typeof(HistoricDetailEntityImpl);
            }
            else
            {
                managedType = managedType ?? clazz;
            }

            var statement = prefix + clazz.Name;
            if (statement.EndsWith("Impl", StringComparison.Ordinal))
            {
                statement = statement.Substring(0, statement.Length - 10); // removing 'entityImpl'
            }
            else
            {
                statement = statement.Substring(0, statement.Length - 6); // removing 'entity'
            }
            return statement;
        }

        // db specific mappings
        // /////////////////////////////////////////////////////

        /// <summary>
        /// 
        /// </summary>
        /// <param name="databaseType"></param>
        /// <param name="activitiStatement"></param>
        /// <param name="ibatisStatement"></param>
        protected internal static void AddDatabaseSpecificStatement(string databaseType, string activitiStatement, string ibatisStatement)
        {
            databaseSpecificStatements.TryGetValue(databaseType, out IDictionary<string, string> specificStatements);
            if (specificStatements is null)
            {
                specificStatements = new Dictionary<string, string>();
                databaseSpecificStatements[databaseType] = specificStatements;
            }
            specificStatements[activitiStatement] = ibatisStatement;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="statement"></param>
        /// <returns></returns>
        public virtual string MapStatement(string statement)
        {
            return statement;
        }

        // customized getters and setters
        // ///////////////////////////////////////////

        /// <summary>
        /// 
        /// </summary>
        public virtual string DatabaseType
        {
            set
            {
                this.databaseType = value;
            }
            get
            {
                return databaseType;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isBulkInsertEnabled"></param>
        /// <param name="databaseType"></param>
        public virtual void SetBulkInsertEnabled(bool isBulkInsertEnabled, string databaseType)
        {
            // If false, just keep don't initialize the map. Memory saved.
            if (isBulkInsertEnabled)
            {
                InitBulkInsertEnabledMap(databaseType);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="databaseType"></param>
        protected internal virtual void InitBulkInsertEnabledMap(string databaseType)
        {
            bulkInsertableMap = new Dictionary<Type, bool>();

            foreach (Type clazz in EntityDependencyOrder.INSERT_ORDER)
            {
                bulkInsertableMap[clazz] = true;
            }

            // Only Oracle is making a fuss in one specific case right now
            if ("oracle".Equals(databaseType))
            {
                bulkInsertableMap[typeof(EventLogEntryEntityImpl)] = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="sqlId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        internal RequestContext CreateRequestContext(string scope, string sqlId, object request)
        {
            var req = new RequestContext()
            {
                Scope = scope,
                SqlId = sqlId,
                Request = request
            };

            return req;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityClass"></param>
        /// <returns></returns>
        public virtual bool? IsBulkInsertable(Type entityClass)
        {
            return bulkInsertableMap is object && bulkInsertableMap.ContainsKey(entityClass) && bulkInsertableMap[entityClass];
        }

        // getters and setters //////////////////////////////////////////////////////

        /// <summary>
        /// 
        /// </summary>
        public virtual IIdGenerator IdGenerator
        {
            get
            {
                return idGenerator;
            }
            set
            {
                this.idGenerator = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool DbHistoryUsed
        {
            get
            {
                return isDbHistoryUsed;
            }
            set
            {
                this.isDbHistoryUsed = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string DatabaseTablePrefix
        {
            set
            {
                this.databaseTablePrefix = value;
            }
            get
            {
                return databaseTablePrefix;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string DatabaseCatalog
        {
            get
            {
                return databaseCatalog;
            }
            set
            {
                this.databaseCatalog = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string DatabaseSchema
        {
            get
            {
                return databaseSchema;
            }
            set
            {
                this.databaseSchema = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool TablePrefixIsSchema
        {
            set
            {
                this.tablePrefixIsSchema = value;
            }
            get
            {
                return tablePrefixIsSchema;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int MaxNrOfStatementsInBulkInsert
        {
            get
            {
                return maxNrOfStatementsInBulkInsert;
            }
            set
            {
                this.maxNrOfStatementsInBulkInsert = value;
            }
        }
    }
}