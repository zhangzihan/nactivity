using System;
using System.Collections.Concurrent;
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

namespace org.activiti.engine.impl.db
{
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using Microsoft.Extensions.DependencyInjection;
    using Sys;
    using Sys.Data;

    /// 
    /// 
    public class DbSqlSessionFactory : ISessionFactory
    {
        protected internal static readonly IDictionary<string, IDictionary<string, string>> databaseSpecificStatements = new Dictionary<string, IDictionary<string, string>>();

        /// <summary>
        /// A map {class, boolean}, to indicate whether or not a certain <seealso cref="IEntity"/> class can be bulk inserted.
        /// </summary>
        protected internal static IDictionary<Type, bool> bulkInsertableMap;

        protected internal string databaseType;
        protected internal string databaseTablePrefix = "";
        private bool tablePrefixIsSchema;

        protected internal string databaseCatalog;
        /// <summary>
        /// In some situations you want to set the schema to use for table checks /
        /// generation if the database metadata doesn't return that correctly, see
        /// https://activiti.atlassian.net/browse/ACT-1220,
        /// https://activiti.atlassian.net/browse/ACT-1062
        /// </summary>
        protected internal string databaseSchema;

        protected internal IIdGenerator idGenerator;

        // Caches, filled while executing processes 
        protected internal bool isDbHistoryUsed = true;
        protected internal int maxNrOfStatementsInBulkInsert = 100;

        public virtual Type SessionType
        {
            get
            {
                return typeof(DbSqlSession);
            }
        }

        public virtual ISession openSession(ICommandContext commandContext)
        {
            DbSqlSession dbSqlSession = new DbSqlSession(this, commandContext.EntityCache);

            return dbSqlSession;
        }

        // insert, update and delete statements
        // /////////////////////////////////////

        public virtual string getInsertStatement(Type clazz, ref Type managedType)
        {
            return getStatement(clazz, "insert", ref managedType);
        }

        public virtual string getBulkInsertStatement(Type clazz, ref Type managedType)
        {
            return getStatement(clazz, "bulkInsert", ref managedType);
        }

        public virtual string getUpdateStatement(Type clazz, ref Type managedType)
        {
            return getStatement(clazz, "update", ref managedType);
        }

        public virtual string getDeleteStatement(Type clazz, ref Type managedType)
        {
            return getStatement(clazz, "delete", ref managedType);
        }

        public virtual string getBulkDeleteStatement(Type clazz, ref Type managedType)
        {
            return getStatement(clazz, "bulkDelete", ref managedType);
        }

        public virtual string getSelectStatement(ref Type entityClass)
        {
            return getStatement(entityClass, "select", ref entityClass);
        }

        private string getStatement(Type clazz, string prefix, ref Type managedType)
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

        protected internal static void addDatabaseSpecificStatement(string databaseType, string activitiStatement, string ibatisStatement)
        {
            databaseSpecificStatements.TryGetValue(databaseType, out IDictionary<string, string> specificStatements);
            if (specificStatements == null)
            {
                specificStatements = new Dictionary<string, string>();
                databaseSpecificStatements[databaseType] = specificStatements;
            }
            specificStatements[activitiStatement] = ibatisStatement;
        }

        public virtual string mapStatement(string statement)
        {
            return statement;
        }

        // customized getters and setters
        // ///////////////////////////////////////////

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

        public virtual void setBulkInsertEnabled(bool isBulkInsertEnabled, string databaseType)
        {
            // If false, just keep don't initialize the map. Memory saved.
            if (isBulkInsertEnabled)
            {
                initBulkInsertEnabledMap(databaseType);
            }
        }

        protected internal virtual void initBulkInsertEnabledMap(string databaseType)
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

        internal SmartSql.Abstractions.RequestContext CreateRequestContext(string scope, string sqlId, object request)
        {
            var req = new SmartSql.Abstractions.RequestContext()
            {
                Scope = scope,
                SqlId = sqlId,
                Request = request
            };

            return req;
        }

        public virtual bool? isBulkInsertable(Type entityClass)
        {
            return bulkInsertableMap != null && bulkInsertableMap.ContainsKey(entityClass) && bulkInsertableMap[entityClass];
        }

        // getters and setters //////////////////////////////////////////////////////

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