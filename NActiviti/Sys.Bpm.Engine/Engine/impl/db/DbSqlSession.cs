using System;
using System.Collections.Generic;
using DatabaseSchemaReader;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sys.Workflow.Engine.Impl.Cfg;
using Sys.Workflow.Engine.Impl.Contexts;
using Sys.Workflow.Engine.Impl.DB.Upgrade;
using Sys.Workflow.Engine.Impl.Interceptor;
using Sys.Workflow.Engine.Impl.Persistence.Caches;
using Sys.Workflow.Engine.Impl.Persistence.Entity;
using Sys.Workflow.Engine.Impl.Util;
using SmartSql.Abstractions;
using SmartSql.Utils;
using Sys.Data;
using Sys.Workflow;
using System.Collections.Concurrent;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Sys.Workflow.Engine.Impl.Variable;

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

    /// <summary>
    /// 
    /// </summary>
    public class DbSqlSession : ISession
    {
        /// <summary>
        /// 
        /// </summary>
        protected internal class DataObject
        {
            private readonly Dictionary<string, IEntity> objects = new Dictionary<string, IEntity>();
            /// <summary>
            /// 
            /// </summary>
            /// <param name="item"></param>
            /// <returns></returns>
            public IEntity this[string item]
            {
                get
                {
                    return objects[item];
                }
                set
                {
                    objects[item] = value;
                }
            }
            /// <summary>
            /// 
            /// </summary>
            public ICollection<IEntity> Values
            {
                get
                {
                    return objects.Values;
                }
            }
            /// <summary>
            /// 
            /// </summary>
            public ICollection<string> Keys
            {
                get
                {
                    return objects.Keys;
                }
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="key"></param>
            /// <returns></returns>
            public bool ContainsKey(string key)
            {
                return objects.ContainsKey(key);
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="key"></param>
            /// <returns></returns>
            public bool Remove(string key)
            {
                return objects.Remove(key);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected internal class DataObjects
        {
            private readonly ConcurrentDictionary<Type, DataObject> objects = new ConcurrentDictionary<Type, DataObject>();
            /// <summary>
            /// 
            /// </summary>
            /// <param name="clazz"></param>
            /// <param name="datas"></param>
            /// <returns></returns>
            public DataObject GetOrAdd(Type clazz, DataObject datas)
            {
                return objects.GetOrAdd(clazz, datas);
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="clazz"></param>
            /// <param name="hisValues"></param>
            /// <returns></returns>
            public bool TryGetValue(Type clazz, out DataObject hisValues)
            {
                return objects.TryGetValue(clazz, out hisValues);
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="clazz"></param>
            /// <param name="value"></param>
            /// <returns></returns>
            public bool TryRemove(Type clazz, out DataObject value)
            {
                return objects.TryRemove(clazz, out value);
            }
            /// <summary>
            /// 
            /// </summary>
            public ICollection<Type> Keys
            {
                get
                {
                    return objects.Keys;
                }
            }
            /// <summary>
            /// 
            /// </summary>
            public ICollection<DataObject> Values
            {
                get
                {
                    return objects.Values;
                }
            }
            /// <summary>
            /// 
            /// </summary>
            public void Clear()
            {
                objects.Clear();
            }
            /// <summary>
            /// 
            /// </summary>
            public int Count
            {
                get
                {
                    return objects.Count;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static readonly Regex CLEAN_VERSION_REGEX = new Regex("\\d\\.\\d*");

        /// <summary>
        /// 
        /// </summary>
        protected internal static readonly IList<ActivitiVersion> ACTIVITI_VERSIONS = DbSqlSessionVersion.ACTIVITI_VERSIONS;

        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
        protected internal DbSqlSessionFactory dbSqlSessionFactory;
        /// <summary>
        /// 
        /// </summary>
        protected internal IEntityCache entityCache;

        private readonly IDbConnection connection;

        /// <summary>
        /// 
        /// </summary>
        protected internal ConcurrentDictionary<Type, Dictionary<string, IEntity>> insertedObjects = new ConcurrentDictionary<Type, Dictionary<string, IEntity>>();
        /// <summary>
        /// 
        /// </summary>
        protected internal ConcurrentDictionary<Type, Dictionary<string, IEntity>> deletedObjects = new ConcurrentDictionary<Type, Dictionary<string, IEntity>>();
        /// <summary>
        /// 
        /// </summary>
        protected internal ConcurrentDictionary<Type, IList<BulkDeleteOperation>> bulkDeleteOperations = new ConcurrentDictionary<Type, IList<BulkDeleteOperation>>();
        /// <summary>
        /// 
        /// </summary>
        protected internal IList<IEntity> updatedObjects = new List<IEntity>();

        /// <summary>
        /// 
        /// </summary>
        protected internal string connectionMetadataDefaultCatalog;
        /// <summary>
        /// 
        /// </summary>
        protected internal string connectionMetadataDefaultSchema;

        /// <summary>
        /// 
        /// </summary>
        public DbSqlSession(DbSqlSessionFactory dbSqlSessionFactory, IEntityCache entityCache)
        {
            this.dbSqlSessionFactory = dbSqlSessionFactory;
            this.entityCache = entityCache;
            this.connectionMetadataDefaultCatalog = dbSqlSessionFactory.DatabaseCatalog;
            this.connectionMetadataDefaultSchema = dbSqlSessionFactory.DatabaseSchema;
        }

        /// <summary>
        /// 
        /// </summary>
        public DbSqlSession(DbSqlSessionFactory dbSqlSessionFactory, IEntityCache entityCache, IDbConnection connection, string catalog, string schema)
        {
            this.dbSqlSessionFactory = dbSqlSessionFactory;
            this.entityCache = entityCache;
            this.connection = connection;
            this.connectionMetadataDefaultCatalog = catalog;
            this.connectionMetadataDefaultSchema = schema;
        }

        // insert ///////////////////////////////////////////////////////////////////

        /// <summary>
        /// 
        /// </summary>
        public virtual void Insert(IEntity entity, Type managedType = null)
        {
            try
            {
                if (entity.Id is null)
                {
                    string id = dbSqlSessionFactory.IdGenerator.GetNextId();
                    entity.Id = id;
                }

                Type clazz = managedType ?? entity.GetType();
                var insObjs = insertedObjects.GetOrAdd(clazz, new Dictionary<string, IEntity>());
                insObjs[entity.Id] = entity; 
                entityCache.Put(entity, false); // False -> entity is inserted, so always changed
                entity.Inserted = true;
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"插入错误managedType={managedType?.GetType()}&entityType={entity?.GetType()}");
                throw ex;
            }
        }

        // update
        // ///////////////////////////////////////////////////////////////////

        /// <summary>
        /// 
        /// </summary>
        public virtual void Update(IEntity entity)
        {
            entityCache.Put(entity, false); // false -> we don't store state, meaning it will always be seen as changed
            entity.Updated = true;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int Update<TEntityImpl>(string statement, object parameters)
        {
            string updateStatement = dbSqlSessionFactory.MapStatement(statement);
            return SqlMapper.Execute(dbSqlSessionFactory.CreateRequestContext(typeof(TEntityImpl).FullName, updateStatement, parameters));//.update(updateStatement, parameters);
        }

        // delete
        // ///////////////////////////////////////////////////////////////////

        /// <summary>
        /// Executes a <seealso cref="BulkDeleteOperation"/>, with the sql in the statement parameter.
        /// The passed class determines when this operation will be executed: it will be executed
        /// when the particular class has passed in the <seealso cref="EntityDependencyOrder"/>.
        /// </summary>
        public virtual void Delete(string statement, object parameter, Type entityClass)
        {
            var lstBulkDel = bulkDeleteOperations.GetOrAdd(entityClass, new List<BulkDeleteOperation>());

            lstBulkDel.Add(new BulkDeleteOperation(dbSqlSessionFactory.MapStatement(statement), parameter));
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual object Delete(IEntity entity)
        {
            Type clazz = entity.GetType();
            var dec = deletedObjects.GetOrAdd(clazz, new Dictionary<string, IEntity>()); // order of insert is important, hence LinkedHashMap
            dec[entity.Id] = entity;
            entity.Deleted = true;

            //if (insertedObjects is object && insertedObjects.TryGetValue(clazz, out var insObj))
            //{
            //    if (insObj.TryGetValue(entity.Id, out IEntity insEntity))
            //    {
            //        insObj.Remove(entity.Id);
            //    }
            //}

            //IEntity updEntity = updatedObjects?.FirstOrDefault(x => string.Compare(x.Id, entity.Id, true) == 0);
            //if (updEntity is object)
            //{
            //    updatedObjects.Remove(updEntity);
            //}
            return entity;
        }

        // select
        // ///////////////////////////////////////////////////////////////////
        /// <summary>
        /// 
        /// </summary>
        public virtual IList<TOut> SelectList<TEntityImpl, TOut>(string statement)
        {
            return SelectList<TEntityImpl, TOut>(statement, null, 0, int.MaxValue);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IList<TOut> SelectList<TEntityImpl, TOut>(string statement, object parameter)
        {
            return SelectList<TEntityImpl, TOut>(statement, parameter, 0, int.MaxValue);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IList<TOut> SelectList<TEntityImpl, TOut>(string statement, object parameter, bool useCache)
        {
            return SelectList<TEntityImpl, TOut>(statement, parameter, 0, int.MaxValue, useCache);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IList<TOut> SelectList<TEntityImpl, TOut>(string statement, object parameter, Page page, bool useCache = true)
        {
            if (page != null)
            {
                return SelectList<TEntityImpl, TOut>(statement, parameter, page.FirstResult, page.MaxResults, useCache);
            }
            else
            {
                return SelectList<TEntityImpl, TOut>(statement, parameter, 0, int.MaxValue, useCache);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IList<TOut> SelectList<TEntityImpl, TOut>(string statement, object parameter, int firstResult, int maxResults, bool useCache = true)
        {
            return SelectList<TEntityImpl, TOut>(statement, new ListQueryParameterObject(parameter, firstResult, maxResults), useCache);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IList<TOut> SelectList<TEntityImpl, TOut>(string statement, ListQueryParameterObject parameter, Page page, bool useCache = true)
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

            return SelectList<TEntityImpl, TOut>(statement, parameterToUse, useCache);
        }

        private IList<TOut> SelectList<TEntityImpl, TOut>(string statement, ListQueryParameterObject parameter, bool useCache = true)
        {
            return SelectListWithRawParameter<TEntityImpl, TOut>(statement, parameter.Parameter, parameter.FirstResult, parameter.MaxResults, useCache);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IList<TOut> SelectListWithRawParameter<TEntityImpl, TOut>(string statement, object parameter, int firstResult, int maxResults, bool useCache = true)
        {
            statement = dbSqlSessionFactory.MapStatement(statement);
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

            if (request.ContainsKey("firstRow") == false)
            {
                request.Add("firstRow", firstResult);
            }

            if (request.ContainsKey("lastRow") == false)
            {
                request.Add("lastRow", firstResult + maxResults + 1);
            }

            RequestContext ctx = dbSqlSessionFactory.CreateRequestContext(typeof(TEntityImpl).FullName, statement, request);

            var loadedObjects = SqlMapper.Query<TOut>(ctx);
            if (useCache)
            {
                var objs = loadedObjects.Cast<object>().ToList();
                return CacheLoadOrStore(objs).Cast<TOut>().ToList();
            }
            else
            {
                return loadedObjects.ToList();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IList<TOut> SelectListWithRawParameterWithoutFilter<TEntityImpl, TOut>(string statement, object parameter, int firstResult, int maxResults)
        {
            statement = dbSqlSessionFactory.MapStatement(statement);
            if (firstResult == -1 || maxResults == -1)
            {
                return new List<TOut>();
            }
            return SelectList<TEntityImpl, TOut>(statement, parameter);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual TOut SelectOne<TEntityImpl, TOut>(string statement, object parameter)
        {
            statement = dbSqlSessionFactory.MapStatement(statement);

            var result = SqlMapper.QuerySingle<TOut>(dbSqlSessionFactory.CreateRequestContext(typeof(TEntityImpl).FullName, statement, parameter));

            if (result is IEntity entity)
            {
                CacheLoadOrStore(entity);
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual TOut SelectById<TEntityImpl, TOut>(KeyValuePair<string, object> id, bool useCache = true) where TEntityImpl : IEntity where TOut : IEntity
        {
            Type entityClass = typeof(TEntityImpl);
            if (id.Value == null)
            {
                return default;
            }

            IEntity entity;
            if (useCache)
            {
                entity = entityCache.FindInCache(entityClass, id.Value.ToString()) as IEntity;
                if (entity != null)
                {
                    return (TOut)entity;
                }
            }

            string selectStatement = dbSqlSessionFactory.GetSelectStatement(ref entityClass);
            selectStatement = dbSqlSessionFactory.MapStatement(selectStatement);

            entity = SqlMapper.QuerySingle<TEntityImpl>(dbSqlSessionFactory.CreateRequestContext(entityClass.FullName, selectStatement, new Dictionary<string, object>()
            {
                { id.Key, id.Value }
            }));

            if (entity == null)
            {
                return default;
            }

            entityCache.Put(entity, true); // true -> store state so we can see later if it is updated later on
            return (TOut)entity;
        }

        // internal session cache
        // ///////////////////////////////////////////////////
        /// <summary>
        /// 
        /// </summary>
        protected virtual IList<object> CacheLoadOrStore(IList<object> loadedObjects)
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
                IEntity cachedEntity = CacheLoadOrStore((IEntity)loadedObject);
                filteredObjects.Add(cachedEntity);
            }
            return filteredObjects;
        }

        /// <summary>
        /// Returns the object in the cache. If this object was loaded before, then the original object is returned (the cached version is more recent).
        /// If this is the first time this object is loaded, then the loadedObject is added to the cache.
        /// </summary>
        protected virtual IEntity CacheLoadOrStore(IEntity entity)
        {
            if (entity == null)
            {
                return null;
            }

            if (entityCache.FindInCache(entity.GetType(), entity.Id) is IEntity cachedEntity)
            {
                return cachedEntity;
            }
            entityCache.Put(entity, true);
            return entity;
        }

        // flush
        // ////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 
        /// </summary>
        public virtual void Flush()
        {
            DetermineUpdatedObjects(); // Needs to be done before the removeUnnecessaryOperations, as removeUnnecessaryOperations will remove stuff from the cache
            RemoveUnnecessaryOperations();

            if (log.IsEnabled(LogLevel.Debug))
            {
                DebugFlush();
            }

            if (SqlMapper.SessionStore.LocalSession == null && (insertedObjects.Count > 0 || updatedObjects.Count > 0 || deletedObjects.Count > 0))
            {
                SqlMapper.BeginTransaction();// IsolationLevel.ReadUncommitted);

                RemoveInstanceIncludeHis();
            }

            FlushInserts();
            FlushUpdates();
            FlushDeletes();
        }

        private void RemoveInstanceIncludeHis()
        {
            if (insertedObjects.Count == 0)
            {
                return;
            }

            if (insertedObjects.TryGetValue(typeof(HistoricIdentityLinkEntityImpl), out var hisValues) &&
                insertedObjects.TryGetValue(typeof(IdentityLinkEntityImpl), out var values))
            {
                for (int idx = values.Keys.Count - 1; idx >= 0; idx--)
                {
                    var key = values.Keys.ElementAt(idx);
                    var val = values[key];

                    if (hisValues.Values.Any(x => x.Id == val.Id))
                    {
                        values.Remove(key);
                    }
                }
            }
        }

        /// <summary>
        /// Clears all deleted and inserted objects from the cache,
        /// and removes inserts and deletes that cancel each other.
        /// <para>
        /// Also removes deletes with duplicate ids.
        /// </para>
        /// </summary>
        protected virtual void RemoveUnnecessaryOperations()
        {
            foreach (Type entityClass in deletedObjects.Keys)
            {
                // Collect ids of deleted entities + remove duplicates
                ISet<string> ids = new HashSet<string>();

                deletedObjects.TryGetValue(entityClass, out var entities);
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

                // Now we have the deleted ids, we can remove the inserted objects (as they cancel each other)
                foreach (string id in ids)
                {
                    if (insertedObjects.TryGetValue(entityClass, out var iec) && iec.ContainsKey(id))
                    {
                        iec.Remove(id);
                        if (deletedObjects.TryGetValue(entityClass, out var dec))
                        {
                            dec.Remove(id);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void DetermineUpdatedObjects()
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

                    if (!IsEntityInserted(cachedEntity) && (cachedEntity.GetType().IsAssignableFrom(typeof(IExecutionEntity)) || !IsEntityToBeDeleted(cachedEntity)) && cachedObject.hasChanged())
                    {
                        updatedObjects.Add(cachedEntity);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void DebugFlush()
        {
            //log.LogDebug("Flushing dbSqlSession");
            //int nrOfInserts = 0, nrOfUpdates = 0, nrOfDeletes = 0;
            //foreach (Dictionary<string, IEntity> insertedObjectMap in insertedObjects.Values)
            //{
            //    foreach (IEntity insertedObject in insertedObjectMap.Values)
            //    {
            //        log.LogDebug($"  insert {insertedObject}");
            //        nrOfInserts++;
            //    }
            //}
            //foreach (IEntity updatedObject in updatedObjects)
            //{
            //    log.LogDebug($"  update {updatedObject}");
            //    nrOfUpdates++;
            //}
            //foreach (Dictionary<string, IEntity> deletedObjectMap in deletedObjects.Values)
            //{
            //    foreach (IEntity deletedObject in deletedObjectMap.Values)
            //    {
            //        log.LogDebug($"  delete {deletedObject} with id {deletedObject.Id}");
            //        nrOfDeletes++;
            //    }
            //}
            //foreach (ICollection<BulkDeleteOperation> bulkDeleteOperationList in bulkDeleteOperations.Values)
            //{
            //    foreach (BulkDeleteOperation bulkDeleteOperation in bulkDeleteOperationList)
            //    {
            //        log.LogDebug($"  {bulkDeleteOperation}");
            //        nrOfDeletes++;
            //    }
            //}
            //log.LogDebug($"flush summary: {nrOfInserts} insert, {nrOfUpdates} update, {nrOfDeletes} delete.");
            //log.LogDebug($"now executing flush...");
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool IsEntityInserted(IEntity entity)
        {
            if (insertedObjects.TryGetValue(entity.GetType(), out var ec))
            {
                return ec.ContainsKey(entity.Id);
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool IsEntityToBeDeleted(IEntity entity)
        {
            if (deletedObjects.TryGetValue(entity.GetType(), out var dec))
            {
                return dec.ContainsKey(entity.Id);
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void FlushInserts()
        {
            if (insertedObjects.Count == 0)
            {
                return;
            }

            // Handle in entity dependency order
            foreach (Type entityClass in EntityDependencyOrder.INSERT_ORDER)
            {
                if (insertedObjects.TryGetValue(entityClass, out var ec))
                {
                    FlushInsertEntities(entityClass, ec.Values);
                    _ = insertedObjects.TryRemove(entityClass, out _);
                }
            }

            // Next, in case of custom entities or we've screwed up and forgotten some entity
            if (insertedObjects.Count > 0)
            {
                foreach (Type entityClass in insertedObjects.Keys)
                {
                    insertedObjects.TryGetValue(entityClass, out var ec);
                    FlushInsertEntities(entityClass, ec.Values);
                }
            }

            insertedObjects.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void FlushInsertEntities(Type entityClass, ICollection<IEntity> entitiesToInsert)
        {
            if (entitiesToInsert.Count == 1)
            {
                FlushRegularInsert(entitiesToInsert.ElementAt(0), entityClass);
            }
            else if (false.Equals(dbSqlSessionFactory.IsBulkInsertable(entityClass)))
            {
                foreach (IEntity entity in entitiesToInsert)
                {
                    FlushRegularInsert(entity, entityClass);
                }
            }
            else
            {
                FlushBulkInsert(entitiesToInsert, entityClass);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual ICollection<IEntity> OrderExecutionEntities(IDictionary<string, IEntity> executionEntities, bool parentBeforeChildExecution)
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

                string parentKey = string.IsNullOrWhiteSpace(parentId) ? superExecutionId : parentId;
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
                    if (parentId is object)
                    {
                        while (parentId is object)
                        {
                            string newParentId = childToParentExecutionMapping[parentId];
                            if (newParentId is null)
                            {
                                break;
                            }
                            parentId = newParentId;
                        }
                    }

                    if (parentId is null)
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

                    CollectChildExecutionsForInsertion(result, parentToChildrenMapping, handledExecutionIds, parentId, parentBeforeChildExecution);
                }
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// <param name="parentToChildrenMapping"></param>
        /// <param name="handledExecutionIds"></param>
        /// <param name="parentId"></param>
        /// <param name="parentBeforeChildExecution"></param>
        protected virtual void CollectChildExecutionsForInsertion(IList<IEntity> result, IDictionary<string, IList<IExecutionEntity>> parentToChildrenMapping, ISet<string> handledExecutionIds, string parentId, bool parentBeforeChildExecution)
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

                CollectChildExecutionsForInsertion(result, parentToChildrenMapping, handledExecutionIds, childExecutionEntity.Id, parentBeforeChildExecution);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="clazz"></param>
        protected virtual void FlushRegularInsert(IEntity entity, Type clazz)
        {
            Type managedType = clazz;
            string insertStatement = dbSqlSessionFactory.GetInsertStatement(clazz, ref managedType);

            if (string.IsNullOrWhiteSpace(insertStatement))
            {
                throw new ActivitiException("no insert statement for " + entity.GetType() + " in the ibatis mapping files");
            }

            //log.LogDebug($"inserting: {entity}");
            SqlMapper.Execute(dbSqlSessionFactory.CreateRequestContext(managedType.FullName, insertStatement, entity));

            // See https://activiti.atlassian.net/browse/ACT-1290
            if (entity is IHasRevision)
            {
                IncrementRevision(entity);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="clazz"></param>
        protected virtual void FlushBulkInsert(ICollection<IEntity> entities, Type clazz)
        {
            if ((entities?.Count).GetValueOrDefault() == 0)
            {
                return;
            }

            bool b = entities.Any(x => x.GetType() == typeof(TaskEntityImpl));

            Type managedType = clazz;
            string insertStatement = dbSqlSessionFactory.GetBulkInsertStatement(clazz, ref managedType);

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
                    IncrementRevision(entityIterator.Current);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="insertedObject"></param>
        protected virtual void IncrementRevision(IEntity insertedObject)
        {
            IHasRevision revisionEntity = (IHasRevision)insertedObject;
            if (revisionEntity.Revision == 0)
            {
                revisionEntity.Revision = revisionEntity.RevisionNext;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void FlushUpdates()
        {
            foreach (IEntity updatedObject in updatedObjects)
            {
                Type managedType = updatedObject.GetType();
                string updateStatement = dbSqlSessionFactory.GetUpdateStatement(managedType, ref managedType);
                updateStatement = dbSqlSessionFactory.MapStatement(updateStatement);

                if (string.IsNullOrWhiteSpace(updateStatement))
                {
                    throw new ActivitiException("no update statement for " + updatedObject.GetType() + " in the ibatis mapping files");
                }

                //log.LogDebug($"updating: {updatedObject}");
                int updatedRecords = SqlMapper.Execute(dbSqlSessionFactory.CreateRequestContext(managedType.FullName, updateStatement, updatedObject));
                if (updatedRecords == 0)
                {
                    log.LogWarning(updatedObject + " was updated by another transaction concurrently");
                    continue;
                    //throw new ActivitiOptimisticLockingException(updatedObject + " was updated by another transaction concurrently");
                }

                // See https://activiti.atlassian.net/browse/ACT-1290
                if (updatedObject is IHasRevision revision)
                {
                    revision.Revision = revision.RevisionNext;
                }
            }
            updatedObjects.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void FlushDeletes()
        {
            if (deletedObjects.Count == 0 && bulkDeleteOperations.Count == 0)
            {
                return;
            }

            // Handle in entity dependency order
            foreach (Type entityClass in EntityDependencyOrder.DELETE_ORDER)
            {
                if (deletedObjects.TryGetValue(entityClass, out var dec))
                {
                    FlushDeleteEntities(entityClass, dec.Values);
                    deletedObjects.TryRemove(entityClass, out _);
                }
                FlushBulkDeletes(entityClass);
            }

            // Next, in case of custom entities or we've screwed up and forgotten some entity
            if (deletedObjects.Count > 0)
            {
                foreach (Type entityClass in deletedObjects.Keys)
                {
                    deletedObjects.TryGetValue(entityClass, out var dec);
                    FlushDeleteEntities(entityClass, dec.Values);
                    FlushBulkDeletes(entityClass);
                }
            }

            deletedObjects.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityClass"></param>
        protected virtual void FlushBulkDeletes(Type entityClass)
        {
            // Bulk deletes
            if (bulkDeleteOperations.TryGetValue(entityClass, out var lstBukDel))
            {
                foreach (BulkDeleteOperation bulkDeleteOperation in lstBukDel)
                {
                    bulkDeleteOperation.Execute(entityClass, SqlMapper);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityClass"></param>
        /// <param name="entitiesToDelete"></param>
        protected virtual void FlushDeleteEntities(Type entityClass, IEnumerable<IEntity> entities)
        {
            IList<IEntity> entitiesToDelete = entities.ToList();
            foreach (IEntity entity in entitiesToDelete)
            {
                Type managedType = entity.GetType();
                string deleteStatement = dbSqlSessionFactory.GetDeleteStatement(managedType, ref managedType);

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

        /// <summary>
        /// 
        /// </summary>
        public virtual void Close()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Commit()
        {
            if (SqlMapper.SessionStore?.LocalSession != null)
            {
                SqlMapper.CommitTransaction();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Rollback()
        {
            if (SqlMapper.SessionStore?.LocalSession != null)
            {
                SqlMapper.RollbackTransaction();
            }
        }

        // schema operations
        // ////////////////////////////////////////////////////////

        /// <summary>
        /// 
        /// </summary>
        public virtual void DbSchemaCheckVersion()
        {
            try
            {
                string dbVersion = DbVersion ?? ProcessEngineConstants.VERSION;
                if (!ProcessEngineConstants.VERSION.Equals(dbVersion))
                {
                    throw new ActivitiWrongDbException(ProcessEngineConstants.VERSION, dbVersion);
                }

                string errorMessage = null;
                if (!IsEngineTablePresent())
                {
                    errorMessage = AddMissingComponent(errorMessage, "engine");
                }
                if (dbSqlSessionFactory.DbHistoryUsed && !IsHistoryTablePresent())
                {
                    errorMessage = AddMissingComponent(errorMessage, "history");
                }

                if (errorMessage is object)
                {
                    throw new ActivitiException("Activiti database problem: " + errorMessage);
                }
            }
            catch (Exception e)
            {
                if (IsMissingTablesException(e))
                {
                    throw new ActivitiException("no activiti tables in db. set <property name=\"databaseSchemaUpdate\" to value=\"true\" or value=\"create-drop\" (use create-drop for testing only!) in bean processEngineConfiguration in activiti.cfg.xml for automatic schema creation", e);
                }
                else
                {
                    throw new ActivitiException("couldn't get db schema version", e);
                }
            }

            log.LogDebug($"activiti db schema check successful");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="missingComponents"></param>
        /// <param name="component"></param>
        /// <returns></returns>
        protected virtual string AddMissingComponent(string missingComponents, string component)
        {
            if (missingComponents is null)
            {
                return "Tables missing for component(s) " + component;
            }
            return missingComponents + ", " + component;
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual string DbVersion
        {
            get
            {
                string selectSchemaVersionStatement = dbSqlSessionFactory.MapStatement("selectDbSchemaVersion");
                return SqlMapper.QuerySingle<string>(dbSqlSessionFactory.CreateRequestContext(typeof(PropertyEntityImpl).FullName, selectSchemaVersionStatement, null));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void DbSchemaCreate()
        {
            if (IsEngineTablePresent())
            {
                string dbVersion = DbVersion;
                if (!ProcessEngineConstants.VERSION.Equals(dbVersion))
                {
                    throw new ActivitiWrongDbException(ProcessEngineConstants.VERSION, dbVersion);
                }
            }
            else
            {
                DbSchemaCreateEngine();
            }

            if (dbSqlSessionFactory.DbHistoryUsed)
            {
                DbSchemaCreateHistory();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void DbSchemaCreateHistory()
        {
            ExecuteMandatorySchemaResource("create", "history");
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void DbSchemaCreateEngine()
        {
            ExecuteMandatorySchemaResource("create", "engine");
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void DbSchemaDrop()
        {
            try
            {
                ExecuteMandatorySchemaResource("drop", "engine");
            }
            catch
            {
                // ignore 
            }
            if (dbSqlSessionFactory.DbHistoryUsed)
            {
                try
                {
                    ExecuteMandatorySchemaResource("drop", "history");
                }
                catch
                {
                    // ignore
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void DbSchemaPrune()
        {
            if (IsHistoryTablePresent() && !dbSqlSessionFactory.DbHistoryUsed)
            {
                ExecuteMandatorySchemaResource("drop", "history");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="component"></param>
        public virtual void ExecuteMandatorySchemaResource(string operation, string component)
        {
            ExecuteSchemaResource(operation, component, GetResourceForDbOperation(operation, operation, component), false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual string DbSchemaUpdate()
        {
            string feedback = null;
            bool isUpgradeNeeded = false;
            int matchingVersionIndex = -1;

            if (IsEngineTablePresent())
            {

                IPropertyEntity dbVersionProperty = SelectById<PropertyEntityImpl, IPropertyEntity>(new KeyValuePair<string, object>("name", "schema.version"));
                string dbVersion = dbVersionProperty.Value;

                // Determine index in the sequence of Activiti releases
                matchingVersionIndex = FindMatchingVersionIndex(dbVersion);

                // Exception when no match was found: unknown/unsupported version
                if (matchingVersionIndex < 0)
                {
                    throw new ActivitiException("Could not update Activiti database schema: unknown version from database: '" + dbVersion + "'");
                }

                isUpgradeNeeded = (matchingVersionIndex != (ACTIVITI_VERSIONS.Count - 1));

                if (isUpgradeNeeded)
                {
                    dbVersionProperty.Value = ProcessEngineConstants.VERSION;

                    IPropertyEntity dbHistoryProperty = SelectById<PropertyEntityImpl, IPropertyEntity>(new KeyValuePair<string, object>("name", "schema.history"));

                    // Set upgrade history
                    string dbHistoryValue = dbHistoryProperty.Value + " upgrade(" + dbVersion + "->" + ProcessEngineConstants.VERSION + ")";
                    dbHistoryProperty.Value = dbHistoryValue;

                    // Engine upgrade
                    DbSchemaUpgrade("engine", matchingVersionIndex);
                    feedback = "upgraded Activiti from " + dbVersion + " to " + ProcessEngineConstants.VERSION;
                }
            }
            else
            {
                DbSchemaCreateEngine();
            }
            if (IsHistoryTablePresent())
            {
                if (isUpgradeNeeded)
                {
                    DbSchemaUpgrade("history", matchingVersionIndex);
                }
            }
            else if (dbSqlSessionFactory.DbHistoryUsed)
            {
                DbSchemaCreateHistory();
            }

            return feedback;
        }

        /// <summary>
        /// Returns the index in the list of <seealso cref="ACTIVITI_VERSIONS"/> matching the provided string version.
        /// Returns -1 if no match can be found.
        /// </summary>
        protected virtual int FindMatchingVersionIndex(string dbVersion)
        {
            int index = 0;
            int matchingVersionIndex = -1;
            while (matchingVersionIndex < 0 && index < ACTIVITI_VERSIONS.Count)
            {
                if (ACTIVITI_VERSIONS[index].Matches(dbVersion))
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual bool IsEngineTablePresent()
        {
            return IsTablePresent("ACT_RU_EXECUTION");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual bool IsHistoryTablePresent()
        {
            return IsTablePresent("ACT_HI_PROCINST");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public virtual bool IsTablePresent(string tableName)
        {
            // ACT-1610: in case the prefix IS the schema itself, we don't add the
            // prefix, since the check is already aware of the schema
            if (!dbSqlSessionFactory.TablePrefixIsSchema)
            {
                tableName = PrependDatabaseTablePrefix(tableName);
            }

            try
            {
                string catalog = this.connectionMetadataDefaultCatalog;
                if (dbSqlSessionFactory.DatabaseCatalog is object && dbSqlSessionFactory.DatabaseCatalog.Length > 0)
                {
                    catalog = dbSqlSessionFactory.DatabaseCatalog;
                }

                string schema = this.connectionMetadataDefaultSchema;
                if (dbSqlSessionFactory.DatabaseSchema is object && dbSqlSessionFactory.DatabaseSchema.Length > 0)
                {
                    schema = dbSqlSessionFactory.DatabaseSchema;
                }

                string databaseType = dbSqlSessionFactory.DatabaseType;

                if ("postgres".Equals(databaseType))
                {
                    tableName = tableName.ToLower();
                }

                if (schema is object && "oracle".Equals(databaseType))
                {
                    schema = schema.ToUpper();
                }

                if (catalog is object && catalog.Length == 0)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="versionInDatabase"></param>
        /// <returns></returns>
        protected virtual bool IsUpgradeNeeded(string versionInDatabase)
        {
            if (ProcessEngineConstants.VERSION.Equals(versionInDatabase))
            {
                return false;
            }

            string cleanDbVersion = GetCleanVersion(versionInDatabase);
            string[] cleanDbVersionSplitted = cleanDbVersion.Split("\\.", true);
            int dbMajorVersion = Convert.ToInt32(cleanDbVersionSplitted[0]);
            int dbMinorVersion = Convert.ToInt32(cleanDbVersionSplitted[1]);

            string cleanEngineVersion = GetCleanVersion(ProcessEngineConstants.VERSION);
            string[] cleanEngineVersionSplitted = cleanEngineVersion.Split("\\.", true);
            int engineMajorVersion = Convert.ToInt32(cleanEngineVersionSplitted[0]);
            int engineMinorVersion = Convert.ToInt32(cleanEngineVersionSplitted[1]);

            if ((dbMajorVersion > engineMajorVersion) || ((dbMajorVersion <= engineMajorVersion) && (dbMinorVersion > engineMinorVersion)))
            {
                throw new ActivitiException("Version of activiti database (" + versionInDatabase + ") is more recent than the engine (" + ProcessEngineConstants.VERSION + ")");
            }
            else if (cleanDbVersion.CompareTo(cleanEngineVersion) == 0)
            {
                // Versions don't match exactly, possibly snapshot is being used
                log.LogWarning($"Engine-version is the same, but not an exact match: {versionInDatabase} vs. {ProcessEngineConstants.VERSION}. Not performing database-upgrade.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="versionString"></param>
        /// <returns></returns>
        protected virtual string GetCleanVersion(string versionString)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        protected virtual string PrependDatabaseTablePrefix(string tableName)
        {
            return dbSqlSessionFactory.DatabaseTablePrefix + tableName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="component"></param>
        /// <param name="currentDatabaseVersionsIndex"></param>
        protected virtual void DbSchemaUpgrade(string component, int currentDatabaseVersionsIndex)
        {
            ActivitiVersion activitiVersion = ACTIVITI_VERSIONS[currentDatabaseVersionsIndex];
            string dbVersion = activitiVersion.MainVersion;
            log.LogInformation($"upgrading activiti {component} schema from {dbVersion} to {ProcessEngineConstants.VERSION}");

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
                ExecuteSchemaResource("upgrade", component, GetResourceForDbOperation("upgrade", "upgradestep." + dbVersion + ".to." + nextVersion, component), true);
                dbVersion = nextVersion;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="operation"></param>
        /// <param name="component"></param>
        /// <returns></returns>
        public virtual string GetResourceForDbOperation(string directory, string operation, string component)
        {
            string databaseType = dbSqlSessionFactory.DatabaseType;
            return $"resources/db/{directory}/activiti.{databaseType}.{operation}.{component}.sql";
            //"org/activiti/db/" + directory + "/activiti." + databaseType + "." + operation + "." + component + ".sql";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="component"></param>
        /// <param name="resourceName"></param>
        /// <param name="isOptional"></param>
        public virtual void ExecuteSchemaResource(string operation, string component, string resourceName, bool isOptional)
        {
            using (Stream inputStream = ReflectUtil.GetResourceAsStream(resourceName))
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
                    ExecuteSchemaResource(operation, component, resourceName, inputStream);
                }
            }
        }

        private void ExecuteSchemaResource(string operation, string component, string resourceName, Stream inputStream)
        {
            log.LogInformation($"performing {operation} on {component} with resource {resourceName}");
            string sqlStatement = null;
            string exceptionSqlStatement = null;
            try
            {
                Exception exception = null;
                byte[] bytes = IoUtil.ReadInputStream(inputStream, resourceName);
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
                string line = ReadNextTrimmedLine(reader);
                bool inOraclePlsqlBlock = false;

                while (line is object)
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
                            dbUpgradeStep = (IDbUpgradeStep)ReflectUtil.Instantiate(upgradestepClassName);
                        }
                        catch (ActivitiException e)
                        {
                            throw new ActivitiException("database update csharp class '" + upgradestepClassName + "' can't be instantiated: " + e.Message, e);
                        }
                        try
                        {
                            log.LogDebug($"executing upgrade step java class {upgradestepClassName}");
                            dbUpgradeStep.Execute(this);
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
                            sqlStatement = AddSqlStatementPiece(sqlStatement, line);
                        }
                        else if ((line.EndsWith(";", StringComparison.Ordinal) && !inOraclePlsqlBlock) || (line.StartsWith("/", StringComparison.Ordinal) && inOraclePlsqlBlock))
                        {

                            if (inOraclePlsqlBlock)
                            {
                                inOraclePlsqlBlock = false;
                            }
                            else
                            {
                                sqlStatement = AddSqlStatementPiece(sqlStatement, line.Substring(0, line.Length - 1));
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
                            sqlStatement = AddSqlStatementPiece(sqlStatement, line);
                        }
                    }

                    line = ReadNextTrimmedLine(reader);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlStatement"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        protected virtual string AddSqlStatementPiece(string sqlStatement, string line)
        {
            if (sqlStatement is null)
            {
                return line;
            }
            return sqlStatement + " \n" + line;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        protected virtual string ReadNextTrimmedLine(StringReader reader)
        {
            string line = reader.ReadLine();
            if (!string.IsNullOrWhiteSpace(line))
            {
                line = line.Trim();
            }
            return line;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        protected virtual bool IsMissingTablesException(Exception e)
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

        /// <summary>
        /// 
        /// </summary>
        public virtual void PerformSchemaOperationsProcessEngineBuild()
        {
            string databaseSchemaUpdate = Context.ProcessEngineConfiguration.DatabaseSchemaUpdate;
            log.LogDebug("Executing performSchemaOperationsProcessEngineBuild with setting " + databaseSchemaUpdate);
#if DEBUG
            if (ProcessEngineConfigurationImpl.DB_SCHEMA_UPDATE_DROP_CREATE.Equals(databaseSchemaUpdate))
            {
                try
                {
                    DbSchemaDrop();
                }
                catch (Exception)
                {
                    // ignore
                }
            }
#endif
            if (ProcessEngineConfiguration.DB_SCHEMA_UPDATE_CREATE_DROP.Equals(databaseSchemaUpdate) ||
                ProcessEngineConfigurationImpl.DB_SCHEMA_UPDATE_DROP_CREATE.Equals(databaseSchemaUpdate) ||
                ProcessEngineConfigurationImpl.DB_SCHEMA_UPDATE_CREATE.Equals(databaseSchemaUpdate))
            {
                DbSchemaCreate();
            }
            else if (ProcessEngineConfiguration.DB_SCHEMA_UPDATE_FALSE.Equals(databaseSchemaUpdate))
            {
                DbSchemaCheckVersion();
            }
            else if (ProcessEngineConfiguration.DB_SCHEMA_UPDATE_TRUE.Equals(databaseSchemaUpdate))
            {
                DbSchemaUpdate();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void PerformSchemaOperationsProcessEngineClose()
        {
            string databaseSchemaUpdate = Context.ProcessEngineConfiguration.DatabaseSchemaUpdate;
            if (ProcessEngineConfiguration.DB_SCHEMA_UPDATE_CREATE_DROP.Equals(databaseSchemaUpdate))
            {
                DbSchemaDrop();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool Mysql
        {
            get
            {
                return dbSqlSessionFactory.DatabaseType.Equals("mysql");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool Oracle
        {
            get
            {
                return dbSqlSessionFactory.DatabaseType.Equals("oracle");
            }
        }

        // query factory methods
        // ////////////////////////////////////////////////////

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual DeploymentQueryImpl CreateDeploymentQuery()
        {
            return new DeploymentQueryImpl();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual ModelQueryImpl CreateModelQueryImpl()
        {
            return new ModelQueryImpl();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual ProcessDefinitionQueryImpl CreateProcessDefinitionQuery()
        {
            return new ProcessDefinitionQueryImpl();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual ProcessInstanceQueryImpl CreateProcessInstanceQuery()
        {
            return new ProcessInstanceQueryImpl();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual ExecutionQueryImpl CreateExecutionQuery()
        {
            return new ExecutionQueryImpl();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual TaskQueryImpl CreateTaskQuery()
        {
            return new TaskQueryImpl();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual JobQueryImpl CreateJobQuery()
        {
            return new JobQueryImpl();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual HistoricProcessInstanceQueryImpl CreateHistoricProcessInstanceQuery()
        {
            return new HistoricProcessInstanceQueryImpl();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual HistoricActivityInstanceQueryImpl CreateHistoricActivityInstanceQuery()
        {
            return new HistoricActivityInstanceQueryImpl();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual HistoricTaskInstanceQueryImpl CreateHistoricTaskInstanceQuery()
        {
            return new HistoricTaskInstanceQueryImpl();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual HistoricDetailQueryImpl CreateHistoricDetailQuery()
        {
            return new HistoricDetailQueryImpl();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual HistoricVariableInstanceQueryImpl CreateHistoricVariableInstanceQuery()
        {
            return new HistoricVariableInstanceQueryImpl();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapperClass"></param>
        /// <returns></returns>
        internal object GetMapper(Type mapperClass)
        {
            return Activator.CreateInstance(mapperClass);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual DbSqlSessionFactory DbSqlSessionFactory
        {
            get
            {
                return dbSqlSessionFactory;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal static class DbSqlSessionVersion
    {
        public static ActivitiVersion VERSION { get; private set; }
        public static readonly IList<ActivitiVersion> ACTIVITI_VERSIONS = new List<ActivitiVersion>();

        internal static void InitVersion(IConfiguration configuration)
        {
            ProcessEngineConstants.VERSION = configuration.GetSection("currentVersion").Value;

            VERSION = new ActivitiVersion(ProcessEngineConstants.VERSION);

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