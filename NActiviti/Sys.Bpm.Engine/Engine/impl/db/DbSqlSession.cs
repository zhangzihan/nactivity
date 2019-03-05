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

namespace org.activiti.engine.impl.db
{
    using DatabaseSchemaReader;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.db.upgrade;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.cache;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;
    using SmartSql.Abstractions;
    using SmartSql.Utils;
    using Sys;
    using Sys.Data;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// 
    /// 
    public class DbSqlSession : ISession
    {
        protected internal static readonly Regex CLEAN_VERSION_REGEX = new Regex("\\d\\.\\d*");

        protected internal static readonly IList<ActivitiVersion> ACTIVITI_VERSIONS = DbSqlSessionVersion.ACTIVITI_VERSIONS;

        public ISmartSqlMapper SqlMapper
        {
            get
            {
                ISmartSqlMapper sqlMapper = ProcessEngineServiceProvider.Resolve<ISmartSqlMapper>();

                var cfg = ProcessEngineServiceProvider.Resolve<ProcessEngineConfiguration>();

                sqlMapper.Variables = cfg.GetProperties();

                return sqlMapper;
            }
        }

        private static readonly ILogger<DbSqlSession> log = ProcessEngineServiceProvider.LoggerService<DbSqlSession>();

        protected internal DbSqlSessionFactory dbSqlSessionFactory;
        protected internal IEntityCache entityCache;

        protected internal Dictionary<Type, Dictionary<string, IEntity>> insertedObjects = new Dictionary<Type, Dictionary<string, IEntity>>();
        protected internal Dictionary<Type, Dictionary<string, IEntity>> deletedObjects = new Dictionary<Type, Dictionary<string, IEntity>>();
        protected internal Dictionary<Type, IList<BulkDeleteOperation>> bulkDeleteOperations = new Dictionary<Type, IList<BulkDeleteOperation>>();
        protected internal IList<IEntity> updatedObjects = new List<IEntity>();

        protected internal string connectionMetadataDefaultCatalog;
        protected internal string connectionMetadataDefaultSchema;

        public DbSqlSession(DbSqlSessionFactory dbSqlSessionFactory, IEntityCache entityCache)
        {
            this.dbSqlSessionFactory = dbSqlSessionFactory;
            this.entityCache = entityCache;
            this.connectionMetadataDefaultCatalog = dbSqlSessionFactory.DatabaseCatalog;
            this.connectionMetadataDefaultSchema = dbSqlSessionFactory.DatabaseSchema;
        }

        public DbSqlSession(DbSqlSessionFactory dbSqlSessionFactory, IEntityCache entityCache, IDbConnection connection, string catalog, string schema)
        {
            this.dbSqlSessionFactory = dbSqlSessionFactory;
            this.entityCache = entityCache;
            this.connectionMetadataDefaultCatalog = catalog;
            this.connectionMetadataDefaultSchema = schema;
        }

        // insert ///////////////////////////////////////////////////////////////////

        public virtual void insert(IEntity entity, Type managedType = null)
        {
            if (ReferenceEquals(entity.Id, null))
            {
                string id = dbSqlSessionFactory.IdGenerator.NextId;
                entity.Id = id;
            }

            Type clazz = managedType ?? entity.GetType();
            insertedObjects.TryGetValue(clazz, out var insObjs);
            if (insObjs == null)
            {
                insObjs = new Dictionary<string, IEntity>();
                insertedObjects[clazz] = insObjs;
            }

            insObjs[entity.Id] = entity;
            entityCache.put(entity, false); // False -> entity is inserted, so always changed
            entity.Inserted = true;
        }

        // update
        // ///////////////////////////////////////////////////////////////////

        public virtual void update(IEntity entity)
        {
            entityCache.put(entity, false); // false -> we don't store state, meaning it will always be seen as changed
            entity.Updated = true;
        }

        public virtual int update<TEntityImpl>(string statement, object parameters)
        {
            string updateStatement = dbSqlSessionFactory.mapStatement(statement);
            return SqlMapper.Execute(dbSqlSessionFactory.CreateRequestContext(typeof(TEntityImpl).FullName, updateStatement, parameters));//.update(updateStatement, parameters);
        }

        // delete
        // ///////////////////////////////////////////////////////////////////

        /// <summary>
        /// Executes a <seealso cref="BulkDeleteOperation"/>, with the sql in the statement parameter.
        /// The passed class determines when this operation will be executed: it will be executed
        /// when the particular class has passed in the <seealso cref="EntityDependencyOrder"/>.
        /// </summary>
        public virtual void delete(string statement, object parameter, Type entityClass)
        {
            if (!bulkDeleteOperations.ContainsKey(entityClass))
            {
                bulkDeleteOperations[entityClass] = new List<BulkDeleteOperation>(1);
            }
            bulkDeleteOperations[entityClass].Add(new BulkDeleteOperation(dbSqlSessionFactory.mapStatement(statement), parameter));
        }

        public virtual object delete(IEntity entity)
        {
            Type clazz = entity.GetType();
            if (!deletedObjects.ContainsKey(clazz))
            {
                deletedObjects[clazz] = new Dictionary<string, IEntity>(); // order of insert is important, hence LinkedHashMap
            }
            deletedObjects[clazz][entity.Id] = entity;
            entity.Deleted = true;

            return entity;
        }

        // select
        // ///////////////////////////////////////////////////////////////////
        public virtual IList<TOut> selectList<TEntityImpl, TOut>(string statement)
        {
            return selectList<TEntityImpl, TOut>(statement, null, 0, int.MaxValue);
        }

        public virtual IList<TOut> selectList<TEntityImpl, TOut>(string statement, object parameter)
        {
            return selectList<TEntityImpl, TOut>(statement, parameter, 0, int.MaxValue);
        }

        public virtual IList<TOut> selectList<TEntityImpl, TOut>(string statement, object parameter, bool useCache)
        {
            return selectList<TEntityImpl, TOut>(statement, parameter, 0, int.MaxValue, useCache);
        }

        public virtual IList<TOut> selectList<TEntityImpl, TOut>(string statement, object parameter, Page page, bool useCache = true)
        {
            if (page != null)
            {
                return selectList<TEntityImpl, TOut>(statement, parameter, page.FirstResult, page.MaxResults, useCache);
            }
            else
            {
                return selectList<TEntityImpl, TOut>(statement, parameter, 0, int.MaxValue, useCache);
            }
        }

        public virtual IList<TOut> selectList<TEntityImpl, TOut>(string statement, object parameter, int firstResult, int maxResults, bool useCache = true)
        {
            return selectList<TEntityImpl, TOut>(statement, new ListQueryParameterObject(parameter, firstResult, maxResults), useCache);
        }

        public virtual IList<TOut> selectList<TEntityImpl, TOut>(string statement, ListQueryParameterObject parameter, Page page, bool useCache = true)
        {

            ListQueryParameterObject parameterToUse = new ListQueryParameterObject()
            {
                Parameter = parameter
            };

            if (page != null)
            {
                parameterToUse.FirstResult = page.FirstResult;
                parameterToUse.MaxResults = page.MaxResults;
            }

            return selectList<TEntityImpl, TOut>(statement, parameterToUse, useCache);
        }

        private IList<TOut> selectList<TEntityImpl, TOut>(string statement, ListQueryParameterObject parameter, bool useCache = true)
        {
            return selectListWithRawParameter<TEntityImpl, TOut>(statement, parameter.Parameter, parameter.FirstResult, parameter.MaxResults, useCache);
        }

        public virtual IList<TOut> selectListWithRawParameter<TEntityImpl, TOut>(string statement, object parameter, int firstResult, int maxResults, bool useCache = true)
        {
            statement = dbSqlSessionFactory.mapStatement(statement);
            if (firstResult == -1 || maxResults == -1)
            {
                return new List<TOut>();
            }

            Dictionary<string, object> request = ObjectUtils.ToDictionary(parameter ?? new { }, true);

            if (request.ContainsKey("firstresult") == false)
            {
                request.Add("firstResult", firstResult);
            }

            if (request.ContainsKey("maxresults") == false)
            {
                request.Add("maxResults", maxResults);
            }

            RequestContext ctx = dbSqlSessionFactory.CreateRequestContext(typeof(TEntityImpl).FullName, statement, request);

            var loadedObjects = SqlMapper.Query<TOut>(ctx);
            if (useCache)
            {
                var objs = loadedObjects.Cast<object>().ToList();
                return cacheLoadOrStore(objs).Cast<TOut>().ToList();
            }
            else
            {
                return loadedObjects.ToList();
            }
        }

        public virtual IList<TOut> selectListWithRawParameterWithoutFilter<TEntityImpl, TOut>(string statement, object parameter, int firstResult, int maxResults)
        {
            statement = dbSqlSessionFactory.mapStatement(statement);
            if (firstResult == -1 || maxResults == -1)
            {
                return new List<TOut>();
            }
            return selectList<TEntityImpl, TOut>(statement, parameter);
        }

        public virtual TOut selectOne<TEntityImpl, TOut>(string statement, object parameter)
        {
            statement = dbSqlSessionFactory.mapStatement(statement);

            var result = SqlMapper.QuerySingle<TOut>(dbSqlSessionFactory.CreateRequestContext(typeof(TEntityImpl).FullName, statement, parameter));

            if (result is IEntity)
            {
                cacheLoadOrStore((IEntity)result);
            }

            return result;
        }

        public virtual TOut selectById<TEntityImpl, TOut>(KeyValuePair<string, object> id, bool useCache = true) where TEntityImpl : IEntity where TOut : IEntity
        {
            Type entityClass = typeof(TEntityImpl);

            IEntity entity = null;

            if (id.Value == null)
            {
                return default(TOut);
            }

            if (useCache)
            {
                entity = entityCache.findInCache(entityClass, id.Value.ToString()) as IEntity;
                if (entity != null)
                {
                    return (TOut)entity;
                }
            }

            string selectStatement = dbSqlSessionFactory.getSelectStatement(ref entityClass);
            selectStatement = dbSqlSessionFactory.mapStatement(selectStatement);

            entity = SqlMapper.QuerySingle<TEntityImpl>(dbSqlSessionFactory.CreateRequestContext(entityClass.FullName, selectStatement, new Dictionary<string, object>()
            {
                { id.Key, id.Value }
            }));

            if (entity == null)
            {
                return default(TOut);
            }

            entityCache.put(entity, true); // true -> store state so we can see later if it is updated later on
            return (TOut)entity;
        }

        // internal session cache
        // ///////////////////////////////////////////////////
        protected internal virtual IList<object> cacheLoadOrStore(IList<object> loadedObjects)
        {
            if (loadedObjects.Count == 0)
            {
                return loadedObjects;
            }
            if (!(loadedObjects[0] is IEntity))
            {
                return loadedObjects;
            }

            IList<object> filteredObjects = new List<object>(loadedObjects.Count);
            foreach (object loadedObject in loadedObjects)
            {
                IEntity cachedEntity = cacheLoadOrStore((IEntity)loadedObject);
                filteredObjects.Add(cachedEntity);
            }
            return filteredObjects;
        }

        /// <summary>
        /// Returns the object in the cache. If this object was loaded before, then the original object is returned (the cached version is more recent).
        /// If this is the first time this object is loaded, then the loadedObject is added to the cache.
        /// </summary>
        protected internal virtual IEntity cacheLoadOrStore(IEntity entity)
        {
            if (entity == null)
            {
                return null;
            }

            IEntity cachedEntity = entityCache.findInCache(entity.GetType(), entity.Id) as IEntity;

            if (cachedEntity != null)
            {
                return cachedEntity;
            }
            entityCache.put(entity, true);
            return entity;
        }

        // flush
        // ////////////////////////////////////////////////////////////////////

        public virtual void flush()
        {
            determineUpdatedObjects(); // Needs to be done before the removeUnnecessaryOperations, as removeUnnecessaryOperations will remove stuff from the cache
            removeUnnecessaryOperations();

            if (log.IsEnabled(LogLevel.Debug))
            {
                debugFlush();
            }

            flushInserts();
            flushUpdates();
            flushDeletes();
        }

        /// <summary>
        /// Clears all deleted and inserted objects from the cache,
        /// and removes inserts and deletes that cancel each other.
        /// <para>
        /// Also removes deletes with duplicate ids.
        /// </para>
        /// </summary>
        protected internal virtual void removeUnnecessaryOperations()
        {
            foreach (Type entityClass in deletedObjects.Keys)
            {
                // Collect ids of deleted entities + remove duplicates
                ISet<string> ids = new HashSet<string>();

                var entities = deletedObjects[entityClass];
                for (var idx = entities.Keys.Count - 1; idx >= 0; idx--)
                {
                    var key = entities.Keys.ElementAt(idx);
                    IEntity entity = entities[key];
                    if (!ids.Contains(entity.Id))
                    {
                        ids.Add(entity.Id);
                    }
                    else
                    {
                        entities.Remove(key); // Removing duplicate deletes
                    }
                }

                //var entitiesToDeleteIterator = deletedObjects[entityClass].Values;
                //for (var idx = entitiesToDeleteIterator.Count - 1; idx >= 0; idx--)
                //{
                //    IEntity entityToDelete = entitiesToDeleteIterator.ElementAt(idx);
                //    if (!ids.Contains(entityToDelete.Id))
                //    {
                //        ids.Add(entityToDelete.Id);
                //    }
                //    else
                //    {
                //        entitiesToDeleteIterator.Remove(entityToDelete); // Removing duplicate deletes
                //    }
                //}

                // Now we have the deleted ids, we can remove the inserted objects (as they cancel each other)
                foreach (string id in ids)
                {
                    if (insertedObjects.ContainsKey(entityClass) && insertedObjects[entityClass].ContainsKey(id))
                    {
                        insertedObjects[entityClass].Remove(id);
                        deletedObjects[entityClass].Remove(id);
                    }
                }
            }
        }

        public virtual void determineUpdatedObjects()
        {
            updatedObjects = new List<IEntity>();
            IDictionary<Type, IDictionary<string, CachedEntity>> cachedObjects = entityCache.AllCachedEntities;
            foreach (Type clazz in cachedObjects.Keys)
            {

                IDictionary<string, CachedEntity> classCache = cachedObjects[clazz];
                foreach (CachedEntity cachedObject in classCache.Values)
                {

                    IEntity cachedEntity = cachedObject.Entity;

                    // Executions are stored as a hierarchical tree, and updates are important to execute
                    // even when the execution are deleted, as they can change the parent-child relationships.
                    // For the other entities, this is not applicable and an update can be discarded when an update follows.

                    if (!isEntityInserted(cachedEntity) && (cachedEntity.GetType().IsAssignableFrom(typeof(IExecutionEntity)) || !isEntityToBeDeleted(cachedEntity)) && cachedObject.hasChanged())
                    {
                        updatedObjects.Add(cachedEntity);
                    }
                }
            }
        }

        protected internal virtual void debugFlush()
        {
            log.LogDebug("Flushing dbSqlSession");
            int nrOfInserts = 0, nrOfUpdates = 0, nrOfDeletes = 0;
            foreach (Dictionary<string, IEntity> insertedObjectMap in insertedObjects.Values)
            {
                foreach (IEntity insertedObject in insertedObjectMap.Values)
                {
                    log.LogDebug($"  insert {insertedObject}");
                    nrOfInserts++;
                }
            }
            foreach (IEntity updatedObject in updatedObjects)
            {
                log.LogDebug($"  update {updatedObject}");
                nrOfUpdates++;
            }
            foreach (Dictionary<string, IEntity> deletedObjectMap in deletedObjects.Values)
            {
                foreach (IEntity deletedObject in deletedObjectMap.Values)
                {
                    log.LogDebug($"  delete {deletedObject} with id {deletedObject.Id}");
                    nrOfDeletes++;
                }
            }
            foreach (ICollection<BulkDeleteOperation> bulkDeleteOperationList in bulkDeleteOperations.Values)
            {
                foreach (BulkDeleteOperation bulkDeleteOperation in bulkDeleteOperationList)
                {
                    log.LogDebug($"  {bulkDeleteOperation}");
                    nrOfDeletes++;
                }
            }
            log.LogDebug($"flush summary: {nrOfInserts} insert, {nrOfUpdates} update, {nrOfDeletes} delete.");
            log.LogDebug($"now executing flush...");
        }

        public virtual bool isEntityInserted(IEntity entity)
        {
            return insertedObjects.ContainsKey(entity.GetType()) && insertedObjects[entity.GetType()].ContainsKey(entity.Id);
        }

        public virtual bool isEntityToBeDeleted(IEntity entity)
        {
            return deletedObjects.ContainsKey(entity.GetType()) && deletedObjects[entity.GetType()].ContainsKey(entity.Id);
        }

        protected internal virtual void flushInserts()
        {
            if (insertedObjects.Count == 0)
            {
                return;
            }

            // Handle in entity dependency order
            foreach (Type entityClass in EntityDependencyOrder.INSERT_ORDER)
            {
                if (insertedObjects.ContainsKey(entityClass))
                {
                    flushInsertEntities(entityClass, insertedObjects[entityClass].Values);
                    insertedObjects.Remove(entityClass);
                }
            }

            // Next, in case of custom entities or we've screwed up and forgotten some entity
            if (insertedObjects.Count > 0)
            {
                foreach (Type entityClass in insertedObjects.Keys)
                {
                    flushInsertEntities(entityClass, insertedObjects[entityClass].Values);
                }
            }

            insertedObjects.Clear();
        }

        protected internal virtual void flushInsertEntities(Type entityClass, ICollection<IEntity> entitiesToInsert)
        {
            if (entitiesToInsert.Count == 1)
            {
                flushRegularInsert(entitiesToInsert.ElementAt(0), entityClass);
            }
            else if (false.Equals(dbSqlSessionFactory.isBulkInsertable(entityClass)))
            {
                foreach (IEntity entity in entitiesToInsert)
                {
                    flushRegularInsert(entity, entityClass);
                }
            }
            else
            {
                flushBulkInsert(entitiesToInsert, entityClass);
            }
        }

        protected internal virtual ICollection<IEntity> orderExecutionEntities(IDictionary<string, IEntity> executionEntities, bool parentBeforeChildExecution)
        {
            // For insertion: parent executions should go before child executions

            IList<IEntity> result = new List<IEntity>(executionEntities.Count);

            // Gather parent-child relationships
            IDictionary<string, string> childToParentExecutionMapping = new Dictionary<string, string>();
            IDictionary<string, IList<IExecutionEntity>> parentToChildrenMapping = new Dictionary<string, IList<IExecutionEntity>>();

            ICollection<IEntity> executionCollection = executionEntities.Values;
            foreach (IExecutionEntity currentExecutionEntity in executionCollection)
            {
                string parentId = currentExecutionEntity.ParentId;
                string superExecutionId = currentExecutionEntity.SuperExecutionId;

                string parentKey = !ReferenceEquals(parentId, null) ? parentId : superExecutionId;
                childToParentExecutionMapping[currentExecutionEntity.Id] = parentKey;

                if (!parentToChildrenMapping.ContainsKey(parentKey))
                {
                    parentToChildrenMapping[parentKey] = new List<IExecutionEntity>();
                }
                parentToChildrenMapping[parentKey].Add(currentExecutionEntity);
            }

            // Loop over all entities, and insert in the correct order
            ISet<string> handledExecutionIds = new HashSet<string>();//executionEntities.Count);
            foreach (IExecutionEntity currentExecutionEntity in executionCollection)
            {
                string executionId = currentExecutionEntity.Id;

                if (!handledExecutionIds.Contains(executionId))
                {
                    string parentId = childToParentExecutionMapping[executionId];
                    if (!ReferenceEquals(parentId, null))
                    {
                        while (!ReferenceEquals(parentId, null))
                        {
                            string newParentId = childToParentExecutionMapping[parentId];
                            if (ReferenceEquals(newParentId, null))
                            {
                                break;
                            }
                            parentId = newParentId;
                        }
                    }

                    if (ReferenceEquals(parentId, null))
                    {
                        parentId = executionId;
                    }

                    if (executionEntities.ContainsKey(parentId) && !handledExecutionIds.Contains(parentId))
                    {
                        handledExecutionIds.Add(parentId);
                        if (parentBeforeChildExecution)
                        {
                            result.Add(executionEntities[parentId]);
                        }
                        else
                        {
                            result.Insert(0, executionEntities[parentId]);
                        }
                    }

                    collectChildExecutionsForInsertion(result, parentToChildrenMapping, handledExecutionIds, parentId, parentBeforeChildExecution);
                }
            }

            return result;
        }

        protected internal virtual void collectChildExecutionsForInsertion(IList<IEntity> result, IDictionary<string, IList<IExecutionEntity>> parentToChildrenMapping, ISet<string> handledExecutionIds, string parentId, bool parentBeforeChildExecution)
        {
            IList<IExecutionEntity> childExecutionEntities = parentToChildrenMapping[parentId];

            if (childExecutionEntities == null)
            {
                return;
            }

            foreach (IExecutionEntity childExecutionEntity in childExecutionEntities)
            {
                handledExecutionIds.Add(childExecutionEntity.Id);
                if (parentBeforeChildExecution)
                {
                    result.Add(childExecutionEntity);
                }
                else
                {
                    result.Insert(0, childExecutionEntity);
                }

                collectChildExecutionsForInsertion(result, parentToChildrenMapping, handledExecutionIds, childExecutionEntity.Id, parentBeforeChildExecution);
            }
        }

        protected internal virtual void flushRegularInsert(IEntity entity, Type clazz)
        {
            Type managedType = clazz;
            string insertStatement = dbSqlSessionFactory.getInsertStatement(clazz, ref managedType);

            if (string.IsNullOrWhiteSpace(insertStatement))
            {
                throw new ActivitiException("no insert statement for " + entity.GetType() + " in the ibatis mapping files");
            }

            log.LogDebug($"inserting: {entity}");
            SqlMapper.Execute(dbSqlSessionFactory.CreateRequestContext(managedType.FullName, insertStatement, entity));

            // See https://activiti.atlassian.net/browse/ACT-1290
            if (entity is IHasRevision)
            {
                incrementRevision(entity);
            }
        }

        protected internal virtual void flushBulkInsert(ICollection<IEntity> entities, Type clazz)
        {
            Type managedType = clazz;
            string insertStatement = dbSqlSessionFactory.getBulkInsertStatement(clazz, ref managedType);

            if (string.IsNullOrWhiteSpace(insertStatement))
            {
                throw new ActivitiException("no insert statement for in the ibatis mapping files");
            }

            IEnumerator<IEntity> entityIterator = entities.GetEnumerator();
            bool? hasRevision = null;

            while (entityIterator.MoveNext())
            {
                IList<IEntity> subList = new List<IEntity>();
                int index = 0;
                do
                {
                    IEntity entity = entityIterator.Current;
                    subList.Add(entity);

                    if (hasRevision == null)
                    {
                        hasRevision = entity is IHasRevision;
                    }
                    index++;
                } while (entityIterator.MoveNext() && index < dbSqlSessionFactory.MaxNrOfStatementsInBulkInsert);

                SqlMapper.Execute(dbSqlSessionFactory.CreateRequestContext(managedType.FullName, insertStatement, new { Items = subList }));
            }

            if (hasRevision.GetValueOrDefault(false))
            {
                entityIterator = entities.GetEnumerator();
                while (entityIterator.MoveNext())
                {
                    incrementRevision(entityIterator.Current);
                }
            }
        }

        protected internal virtual void incrementRevision(IEntity insertedObject)
        {
            IHasRevision revisionEntity = (IHasRevision)insertedObject;
            if (revisionEntity.Revision == 0)
            {
                revisionEntity.Revision = revisionEntity.RevisionNext;
            }
        }

        protected internal virtual void flushUpdates()
        {
            foreach (IEntity updatedObject in updatedObjects)
            {
                Type managedType = updatedObject.GetType();
                string updateStatement = dbSqlSessionFactory.getUpdateStatement(managedType, ref managedType);
                updateStatement = dbSqlSessionFactory.mapStatement(updateStatement);

                if (string.IsNullOrWhiteSpace(updateStatement))
                {
                    throw new ActivitiException("no update statement for " + updatedObject.GetType() + " in the ibatis mapping files");
                }

                log.LogDebug($"updating: {updatedObject}");
                int updatedRecords = SqlMapper.Execute(dbSqlSessionFactory.CreateRequestContext(managedType.FullName, updateStatement, updatedObject));
                if (updatedRecords == 0)
                {
                    log.LogWarning(updatedObject + " was updated by another transaction concurrently");
                    continue;
                    //throw new ActivitiOptimisticLockingException(updatedObject + " was updated by another transaction concurrently");
                }

                // See https://activiti.atlassian.net/browse/ACT-1290
                if (updatedObject is IHasRevision)
                {
                    ((IHasRevision)updatedObject).Revision = ((IHasRevision)updatedObject).RevisionNext;
                }
            }
            updatedObjects.Clear();
        }

        protected internal virtual void flushDeletes()
        {
            if (deletedObjects.Count == 0 && bulkDeleteOperations.Count == 0)
            {
                return;
            }

            // Handle in entity dependency order
            foreach (Type entityClass in EntityDependencyOrder.DELETE_ORDER)
            {
                if (deletedObjects.ContainsKey(entityClass))
                {
                    flushDeleteEntities(entityClass, deletedObjects[entityClass].Values);
                    deletedObjects.Remove(entityClass);
                }
                flushBulkDeletes(entityClass);
            }

            // Next, in case of custom entities or we've screwed up and forgotten some entity
            if (deletedObjects.Count > 0)
            {
                foreach (Type entityClass in deletedObjects.Keys)
                {
                    flushDeleteEntities(entityClass, deletedObjects[entityClass].Values);
                    flushBulkDeletes(entityClass);
                }
            }

            deletedObjects.Clear();
        }

        protected internal virtual void flushBulkDeletes(Type entityClass)
        {
            // Bulk deletes
            if (bulkDeleteOperations.ContainsKey(entityClass))
            {
                foreach (BulkDeleteOperation bulkDeleteOperation in bulkDeleteOperations[entityClass])
                {
                    bulkDeleteOperation.execute(entityClass, SqlMapper);
                }
            }
        }

        protected internal virtual void flushDeleteEntities(Type entityClass, ICollection<IEntity> entitiesToDelete)
        {
            foreach (IEntity entity in entitiesToDelete)
            {
                Type managedType = entity.GetType();
                string deleteStatement = dbSqlSessionFactory.getDeleteStatement(managedType, ref managedType);

                if (string.IsNullOrWhiteSpace(deleteStatement))
                {
                    throw new ActivitiException("no delete statement for " + entity.GetType() + " in the ibatis mapping files");
                }

                // It only makes sense to check for optimistic locking exceptions
                // for objects that actually have a revision

                int nrOfRowsDeleted = SqlMapper.Execute(dbSqlSessionFactory.CreateRequestContext(managedType.FullName, deleteStatement, entity));

                if (entity is IHasRevision && nrOfRowsDeleted == 0)
                {
                    log.LogWarning(entity + " was updated by another transaction concurrently");
                    //throw new ActivitiOptimisticLockingException(entity + " was updated by another transaction concurrently");
                }
            }
        }

        public virtual void close()
        {
        }

        public virtual void commit()
        {
            if (SqlMapper.SessionStore?.LocalSession != null)
            {
                SqlMapper.CommitTransaction();
            }
        }

        public virtual void rollback()
        {
            if (SqlMapper.SessionStore?.LocalSession != null)
            {
                SqlMapper.RollbackTransaction();
            }
        }

        // schema operations
        // ////////////////////////////////////////////////////////

        public virtual void dbSchemaCheckVersion()
        {
            try
            {
                string dbVersion = DbVersion ?? ProcessEngine_Fields.VERSION;
                if (!ProcessEngine_Fields.VERSION.Equals(dbVersion))
                {
                    throw new ActivitiWrongDbException(ProcessEngine_Fields.VERSION, dbVersion);
                }

                string errorMessage = null;
                if (!IsEngineTablePresent())
                {
                    errorMessage = addMissingComponent(errorMessage, "engine");
                }
                if (dbSqlSessionFactory.DbHistoryUsed && !IsHistoryTablePresent())
                {
                    errorMessage = addMissingComponent(errorMessage, "history");
                }

                if (!ReferenceEquals(errorMessage, null))
                {
                    throw new ActivitiException("Activiti database problem: " + errorMessage);
                }
            }
            catch (Exception e)
            {
                if (isMissingTablesException(e))
                {
                    throw new ActivitiException("no activiti tables in db. set <property name=\"databaseSchemaUpdate\" to value=\"true\" or value=\"create-drop\" (use create-drop for testing only!) in bean processEngineConfiguration in activiti.cfg.xml for automatic schema creation", e);
                }
                else
                {
                    if (e is Exception)
                    {
                        throw (Exception)e;
                    }
                    else
                    {
                        throw new ActivitiException("couldn't get db schema version", e);
                    }
                }
            }

            log.LogDebug($"activiti db schema check successful");
        }

        protected internal virtual string addMissingComponent(string missingComponents, string component)
        {
            if (ReferenceEquals(missingComponents, null))
            {
                return "Tables missing for component(s) " + component;
            }
            return missingComponents + ", " + component;
        }

        protected internal virtual string DbVersion
        {
            get
            {
                string selectSchemaVersionStatement = dbSqlSessionFactory.mapStatement("selectDbSchemaVersion");
                return SqlMapper.QuerySingle<string>(dbSqlSessionFactory.CreateRequestContext(typeof(PropertyEntityImpl).FullName, selectSchemaVersionStatement, null));
            }
        }

        public virtual void dbSchemaCreate()
        {
            if (IsEngineTablePresent())
            {
                string dbVersion = DbVersion;
                if (!ProcessEngine_Fields.VERSION.Equals(dbVersion))
                {
                    throw new ActivitiWrongDbException(ProcessEngine_Fields.VERSION, dbVersion);
                }
            }
            else
            {
                dbSchemaCreateEngine();
            }

            if (dbSqlSessionFactory.DbHistoryUsed)
            {
                dbSchemaCreateHistory();
            }
        }

        protected internal virtual void dbSchemaCreateHistory()
        {
            executeMandatorySchemaResource("create", "history");
        }

        protected internal virtual void dbSchemaCreateEngine()
        {
            executeMandatorySchemaResource("create", "engine");
        }

        public virtual void dbSchemaDrop()
        {
            executeMandatorySchemaResource("drop", "engine");
            if (dbSqlSessionFactory.DbHistoryUsed)
            {
                executeMandatorySchemaResource("drop", "history");
            }
        }

        public virtual void dbSchemaPrune()
        {
            if (IsHistoryTablePresent() && !dbSqlSessionFactory.DbHistoryUsed)
            {
                executeMandatorySchemaResource("drop", "history");
            }
        }

        public virtual void executeMandatorySchemaResource(string operation, string component)
        {
            executeSchemaResource(operation, component, getResourceForDbOperation(operation, operation, component), false);
        }

        public virtual string dbSchemaUpdate()
        {
            string feedback = null;
            bool isUpgradeNeeded = false;
            int matchingVersionIndex = -1;

            if (IsEngineTablePresent())
            {

                IPropertyEntity dbVersionProperty = selectById<PropertyEntityImpl, IPropertyEntity>(new KeyValuePair<string, object>("name", "schema.version"));
                string dbVersion = dbVersionProperty.Value;

                // Determine index in the sequence of Activiti releases
                matchingVersionIndex = findMatchingVersionIndex(dbVersion);

                // Exception when no match was found: unknown/unsupported version
                if (matchingVersionIndex < 0)
                {
                    throw new ActivitiException("Could not update Activiti database schema: unknown version from database: '" + dbVersion + "'");
                }

                isUpgradeNeeded = (matchingVersionIndex != (ACTIVITI_VERSIONS.Count - 1));

                if (isUpgradeNeeded)
                {
                    dbVersionProperty.Value = ProcessEngine_Fields.VERSION;

                    IPropertyEntity dbHistoryProperty = selectById<PropertyEntityImpl, IPropertyEntity>(new KeyValuePair<string, object>("name", "schema.history"));

                    // Set upgrade history
                    string dbHistoryValue = dbHistoryProperty.Value + " upgrade(" + dbVersion + "->" + ProcessEngine_Fields.VERSION + ")";
                    dbHistoryProperty.Value = dbHistoryValue;

                    // Engine upgrade
                    dbSchemaUpgrade("engine", matchingVersionIndex);
                    feedback = "upgraded Activiti from " + dbVersion + " to " + ProcessEngine_Fields.VERSION;
                }
            }
            else
            {
                dbSchemaCreateEngine();
            }
            if (IsHistoryTablePresent())
            {
                if (isUpgradeNeeded)
                {
                    dbSchemaUpgrade("history", matchingVersionIndex);
                }
            }
            else if (dbSqlSessionFactory.DbHistoryUsed)
            {
                dbSchemaCreateHistory();
            }

            return feedback;
        }

        /// <summary>
        /// Returns the index in the list of <seealso cref="#ACTIVITI_VERSIONS"/> matching the provided string version.
        /// Returns -1 if no match can be found.
        /// </summary>
        protected internal virtual int findMatchingVersionIndex(string dbVersion)
        {
            int index = 0;
            int matchingVersionIndex = -1;
            while (matchingVersionIndex < 0 && index < ACTIVITI_VERSIONS.Count)
            {
                if (ACTIVITI_VERSIONS[index].matches(dbVersion))
                {
                    matchingVersionIndex = index;
                }
                else
                {
                    index++;
                }
            }
            return matchingVersionIndex;
        }

        public virtual bool IsEngineTablePresent()
        {
            return isTablePresent("ACT_RU_EXECUTION");
        }

        public virtual bool IsHistoryTablePresent()
        {
            return isTablePresent("ACT_HI_PROCINST");
        }

        public virtual bool isTablePresent(string tableName)
        {
            // ACT-1610: in case the prefix IS the schema itself, we don't add the
            // prefix, since the check is already aware of the schema
            if (!dbSqlSessionFactory.TablePrefixIsSchema)
            {
                tableName = prependDatabaseTablePrefix(tableName);
            }

            try
            {
                string catalog = this.connectionMetadataDefaultCatalog;
                if (!ReferenceEquals(dbSqlSessionFactory.DatabaseCatalog, null) && dbSqlSessionFactory.DatabaseCatalog.Length > 0)
                {
                    catalog = dbSqlSessionFactory.DatabaseCatalog;
                }

                string schema = this.connectionMetadataDefaultSchema;
                if (!ReferenceEquals(dbSqlSessionFactory.DatabaseSchema, null) && dbSqlSessionFactory.DatabaseSchema.Length > 0)
                {
                    schema = dbSqlSessionFactory.DatabaseSchema;
                }

                string databaseType = dbSqlSessionFactory.DatabaseType;

                if ("postgres".Equals(databaseType))
                {
                    tableName = tableName.ToLower();
                }

                if (!ReferenceEquals(schema, null) && "oracle".Equals(databaseType))
                {
                    schema = schema.ToUpper();
                }

                if (!ReferenceEquals(catalog, null) && catalog.Length == 0)
                {
                    catalog = null;
                }

                var dbreader = ProcessEngineServiceProvider.Resolve<IDatabaseReader>();

                return dbreader.TableList().Any(x => string.Compare(x.Name, tableName, true) == 0);
            }
            catch (Exception e)
            {
                throw new ActivitiException("couldn't check if tables are already present using metadata: " + e.Message, e);
            }
        }

        protected internal virtual bool isUpgradeNeeded(string versionInDatabase)
        {
            if (ProcessEngine_Fields.VERSION.Equals(versionInDatabase))
            {
                return false;
            }

            string cleanDbVersion = getCleanVersion(versionInDatabase);
            string[] cleanDbVersionSplitted = cleanDbVersion.Split("\\.", true);
            int dbMajorVersion = Convert.ToInt32(cleanDbVersionSplitted[0]);
            int dbMinorVersion = Convert.ToInt32(cleanDbVersionSplitted[1]);

            string cleanEngineVersion = getCleanVersion(ProcessEngine_Fields.VERSION);
            string[] cleanEngineVersionSplitted = cleanEngineVersion.Split("\\.", true);
            int engineMajorVersion = Convert.ToInt32(cleanEngineVersionSplitted[0]);
            int engineMinorVersion = Convert.ToInt32(cleanEngineVersionSplitted[1]);

            if ((dbMajorVersion > engineMajorVersion) || ((dbMajorVersion <= engineMajorVersion) && (dbMinorVersion > engineMinorVersion)))
            {
                throw new ActivitiException("Version of activiti database (" + versionInDatabase + ") is more recent than the engine (" + ProcessEngine_Fields.VERSION + ")");
            }
            else if (cleanDbVersion.CompareTo(cleanEngineVersion) == 0)
            {
                // Versions don't match exactly, possibly snapshot is being used
                log.LogWarning($"Engine-version is the same, but not an exact match: {versionInDatabase} vs. {ProcessEngine_Fields.VERSION}. Not performing database-upgrade.");
                return false;
            }
            return true;
        }

        protected internal virtual string getCleanVersion(string versionString)
        {
            if (!CLEAN_VERSION_REGEX.IsMatch(versionString))
            {
                throw new ActivitiException("Illegal format for version: " + versionString);
            }

            string cleanString = CLEAN_VERSION_REGEX.Match(versionString).Value;
            try
            {
                double.Parse(cleanString); // try to parse it, to see if it is
                                           // really a number
                return cleanString;
            }
            catch (System.FormatException)
            {
                throw new ActivitiException("Illegal format for version: " + versionString);
            }
        }

        protected internal virtual string prependDatabaseTablePrefix(string tableName)
        {
            return dbSqlSessionFactory.DatabaseTablePrefix + tableName;
        }

        protected internal virtual void dbSchemaUpgrade(string component, int currentDatabaseVersionsIndex)
        {
            ActivitiVersion activitiVersion = ACTIVITI_VERSIONS[currentDatabaseVersionsIndex];
            string dbVersion = activitiVersion.MainVersion;
            log.LogInformation($"upgrading activiti {component} schema from {dbVersion} to {ProcessEngine_Fields.VERSION}");

            // Actual execution of schema DDL SQL
            for (int i = currentDatabaseVersionsIndex + 1; i < ACTIVITI_VERSIONS.Count; i++)
            {
                string nextVersion = ACTIVITI_VERSIONS[i].MainVersion;

                // Taking care of -SNAPSHOT version in development
                if (nextVersion.EndsWith("-SNAPSHOT", StringComparison.Ordinal))
                {
                    nextVersion = nextVersion.Substring(0, nextVersion.Length - "-SNAPSHOT".Length);
                }

                dbVersion = dbVersion.Replace(".", "");
                nextVersion = nextVersion.Replace(".", "");
                log.LogInformation($"Upgrade needed: {dbVersion} -> {nextVersion}. Looking for schema update resource for component '{component}'");
                executeSchemaResource("upgrade", component, getResourceForDbOperation("upgrade", "upgradestep." + dbVersion + ".to." + nextVersion, component), true);
                dbVersion = nextVersion;
            }
        }

        public virtual string getResourceForDbOperation(string directory, string operation, string component)
        {
            string databaseType = dbSqlSessionFactory.DatabaseType;
            return $"resources/db/{directory}/activiti.{databaseType}.{operation}.{component}.sql";
            //"org/activiti/db/" + directory + "/activiti." + databaseType + "." + operation + "." + component + ".sql";
        }

        public virtual void executeSchemaResource(string operation, string component, string resourceName, bool isOptional)
        {
            using (Stream inputStream = ReflectUtil.getResourceAsStream(resourceName))
            {
                if (inputStream == null)
                {
                    if (isOptional)
                    {
                        log.LogInformation($"no schema resource {resourceName} for {operation}");
                    }
                    else
                    {
                        throw new ActivitiException("resource '" + resourceName + "' is not available");
                    }
                }
                else
                {
                    executeSchemaResource(operation, component, resourceName, inputStream);
                }
            }
        }

        private void executeSchemaResource(string operation, string component, string resourceName, Stream inputStream)
        {
            log.LogInformation($"performing {operation} on {component} with resource {resourceName}");
            string sqlStatement = null;
            string exceptionSqlStatement = null;
            try
            {
                Exception exception = null;
                byte[] bytes = IoUtil.readInputStream(inputStream, resourceName);
                string ddlStatements = StringHelper.NewString(bytes);

                // Special DDL handling for certain databases
                //try
                //{
                //    if (Mysql)
                //    {
                //        DatabaseMetaData databaseMetaData = connection.MetaData;
                //        int majorVersion = databaseMetaData.DatabaseMajorVersion;
                //        int minorVersion = databaseMetaData.DatabaseMinorVersion;
                //        //log.info("Found MySQL: majorVersion=" + majorVersion + " minorVersion=" + minorVersion);

                //        // Special care for MySQL < 5.6
                //        if (majorVersion <= 5 && minorVersion < 6)
                //        {
                //            ddlStatements = updateDdlForMySqlVersionLowerThan56(ddlStatements);
                //        }
                //    }
                //}
                //catch (Exception e)
                //{
                //    //log.info("Could not get database metadata", e);
                //}

                IDataSource ds = ProcessEngineServiceProvider.Resolve<IDataSource>();

                StringReader reader = new StringReader(ddlStatements);
                string line = readNextTrimmedLine(reader);
                bool inOraclePlsqlBlock = false;

                while (!string.ReferenceEquals(line, null))
                {
                    if (line.StartsWith("# ", StringComparison.Ordinal))
                    {
                        log.LogDebug(line.Substring(2));
                    }
                    else if (line.StartsWith("-- ", StringComparison.Ordinal))
                    {
                        log.LogDebug(line.Substring(3));
                    }
                    else if (line.StartsWith("execute class ", StringComparison.Ordinal))
                    {
                        string upgradestepClassName = line.Substring("execute class ".Length).Trim();
                        IDbUpgradeStep dbUpgradeStep = null;
                        try
                        {
                            dbUpgradeStep = (IDbUpgradeStep)ReflectUtil.instantiate(upgradestepClassName);
                        }
                        catch (ActivitiException e)
                        {
                            throw new ActivitiException("database update csharp class '" + upgradestepClassName + "' can't be instantiated: " + e.Message, e);
                        }
                        try
                        {
                            log.LogDebug($"executing upgrade step java class {upgradestepClassName}");
                            dbUpgradeStep.execute(this);
                        }
                        catch (Exception e)
                        {
                            throw new ActivitiException("error while executing database update csharp class '" + upgradestepClassName + "': " + e.Message, e);
                        }
                    }
                    else if (line.Length > 0)
                    {
                        if (Oracle && line.StartsWith("begin", StringComparison.Ordinal))
                        {
                            inOraclePlsqlBlock = true;
                            sqlStatement = addSqlStatementPiece(sqlStatement, line);
                        }
                        else if ((line.EndsWith(";", StringComparison.Ordinal) && !inOraclePlsqlBlock) || (line.StartsWith("/", StringComparison.Ordinal) && inOraclePlsqlBlock))
                        {

                            if (inOraclePlsqlBlock)
                            {
                                inOraclePlsqlBlock = false;
                            }
                            else
                            {
                                sqlStatement = addSqlStatementPiece(sqlStatement, line.Substring(0, line.Length - 1));
                            }

                            IDbCommand command = ds.Connection.CreateCommand();
                            command.CommandText = sqlStatement;
                            command.CommandType = CommandType.Text;
                            command.Connection.Open();
                            try
                            {
                                // no logging needed as the connection will log it
                                log.LogDebug($"SQL {sqlStatement}");
                                command.ExecuteNonQuery();
                            }
                            catch (Exception e)
                            {
                                if (exception == null)
                                {
                                    exception = e;
                                    exceptionSqlStatement = sqlStatement;
                                }
                                log.LogError(e, $"problem during schema {operation}, statement {sqlStatement}");
                            }
                            finally
                            {
                                command.Connection.Dispose();
                                sqlStatement = null;
                            }
                        }
                        else
                        {
                            sqlStatement = addSqlStatementPiece(sqlStatement, line);
                        }
                    }

                    line = readNextTrimmedLine(reader);
                }

                if (exception != null)
                {
                    throw exception;
                }

                log.LogDebug($"activiti db schema {operation} for component {component} successful");
            }
            catch (Exception e)
            {
                throw new ActivitiException("couldn't " + operation + " db schema: " + exceptionSqlStatement, e);
            }
        }

        protected internal virtual string addSqlStatementPiece(string sqlStatement, string line)
        {
            if (ReferenceEquals(sqlStatement, null))
            {
                return line;
            }
            return sqlStatement + " \n" + line;
        }

        protected internal virtual string readNextTrimmedLine(StringReader reader)
        {
            string line = reader.ReadLine();
            if (!ReferenceEquals(line, null))
            {
                line = line.Trim();
            }
            return line;
        }

        protected internal virtual bool isMissingTablesException(Exception e)
        {
            string exceptionMessage = e.Message;
            if (e.Message != null)
            {
                // Matches message returned from H2
                if ((exceptionMessage.IndexOf("Table", StringComparison.Ordinal) != -1) && (exceptionMessage.IndexOf("not found", StringComparison.Ordinal) != -1))
                {
                    return true;
                }

                // Message returned from MySQL and Oracle
                if (((exceptionMessage.IndexOf("Table", StringComparison.Ordinal) != -1 || exceptionMessage.IndexOf("table", StringComparison.Ordinal) != -1)) && (exceptionMessage.IndexOf("doesn't exist", StringComparison.Ordinal) != -1))
                {
                    return true;
                }

                // Message returned from Postgres
                if (((exceptionMessage.IndexOf("relation", StringComparison.Ordinal) != -1 || exceptionMessage.IndexOf("table", StringComparison.Ordinal) != -1)) && (exceptionMessage.IndexOf("does not exist", StringComparison.Ordinal) != -1))
                {
                    return true;
                }
            }
            return false;
        }

        public virtual void performSchemaOperationsProcessEngineBuild()
        {
            string databaseSchemaUpdate = Context.ProcessEngineConfiguration.DatabaseSchemaUpdate;
            log.LogDebug("Executing performSchemaOperationsProcessEngineBuild with setting " + databaseSchemaUpdate);
            if (ProcessEngineConfigurationImpl.DB_SCHEMA_UPDATE_DROP_CREATE.Equals(databaseSchemaUpdate))
            {
                try
                {
                    dbSchemaDrop();
                }
                catch (Exception)
                {
                    // ignore
                }
            }
            if (ProcessEngineConfiguration.DB_SCHEMA_UPDATE_CREATE_DROP.Equals(databaseSchemaUpdate) || ProcessEngineConfigurationImpl.DB_SCHEMA_UPDATE_DROP_CREATE.Equals(databaseSchemaUpdate) || ProcessEngineConfigurationImpl.DB_SCHEMA_UPDATE_CREATE.Equals(databaseSchemaUpdate))
            {
                dbSchemaCreate();
            }
            else if (ProcessEngineConfiguration.DB_SCHEMA_UPDATE_FALSE.Equals(databaseSchemaUpdate))
            {
                dbSchemaCheckVersion();
            }
            else if (ProcessEngineConfiguration.DB_SCHEMA_UPDATE_TRUE.Equals(databaseSchemaUpdate))
            {
                dbSchemaUpdate();
            }
        }

        public virtual void performSchemaOperationsProcessEngineClose()
        {
            string databaseSchemaUpdate = Context.ProcessEngineConfiguration.DatabaseSchemaUpdate;
            if (ProcessEngineConfiguration.DB_SCHEMA_UPDATE_CREATE_DROP.Equals(databaseSchemaUpdate))
            {
                dbSchemaDrop();
            }
        }

        public virtual bool Mysql
        {
            get
            {
                return dbSqlSessionFactory.DatabaseType.Equals("mysql");
            }
        }

        public virtual bool Oracle
        {
            get
            {
                return dbSqlSessionFactory.DatabaseType.Equals("oracle");
            }
        }

        // query factory methods
        // ////////////////////////////////////////////////////

        public virtual DeploymentQueryImpl createDeploymentQuery()
        {
            return new DeploymentQueryImpl();
        }

        public virtual ModelQueryImpl createModelQueryImpl()
        {
            return new ModelQueryImpl();
        }

        public virtual ProcessDefinitionQueryImpl createProcessDefinitionQuery()
        {
            return new ProcessDefinitionQueryImpl();
        }

        public virtual ProcessInstanceQueryImpl createProcessInstanceQuery()
        {
            return new ProcessInstanceQueryImpl();
        }

        public virtual ExecutionQueryImpl createExecutionQuery()
        {
            return new ExecutionQueryImpl();
        }

        public virtual TaskQueryImpl createTaskQuery()
        {
            return new TaskQueryImpl();
        }

        public virtual JobQueryImpl createJobQuery()
        {
            return new JobQueryImpl();
        }

        public virtual HistoricProcessInstanceQueryImpl createHistoricProcessInstanceQuery()
        {
            return new HistoricProcessInstanceQueryImpl();
        }

        public virtual HistoricActivityInstanceQueryImpl createHistoricActivityInstanceQuery()
        {
            return new HistoricActivityInstanceQueryImpl();
        }

        public virtual HistoricTaskInstanceQueryImpl createHistoricTaskInstanceQuery()
        {
            return new HistoricTaskInstanceQueryImpl();
        }

        public virtual HistoricDetailQueryImpl createHistoricDetailQuery()
        {
            return new HistoricDetailQueryImpl();
        }

        public virtual HistoricVariableInstanceQueryImpl createHistoricVariableInstanceQuery()
        {
            return new HistoricVariableInstanceQueryImpl();
        }

        internal object getMapper(Type mapperClass)
        {
            return Activator.CreateInstance(mapperClass);
        }

        public virtual DbSqlSessionFactory DbSqlSessionFactory
        {
            get
            {
                return dbSqlSessionFactory;
            }
        }
    }

    internal static class DbSqlSessionVersion
    {
        public static ActivitiVersion VERSION { get; private set; }
        public static readonly IList<ActivitiVersion> ACTIVITI_VERSIONS = new List<ActivitiVersion>();

        internal static void InitVersion(IConfiguration configuration)
        {
            ProcessEngine_Fields.VERSION = configuration.GetSection("currentVersion").Value;

            VERSION = new ActivitiVersion(ProcessEngine_Fields.VERSION);

            IEnumerable<IConfigurationSection> historyVersions = configuration.GetSection("historyVersions")?.GetChildren();

            foreach (var v in historyVersions ?? new IConfigurationSection[0])
            {
                if (string.IsNullOrWhiteSpace(v.Value) == false)
                {
                    ACTIVITI_VERSIONS.Add(new ActivitiVersion(v.Value));
                }
            }

            ACTIVITI_VERSIONS.Add(VERSION);
        }
    }
}