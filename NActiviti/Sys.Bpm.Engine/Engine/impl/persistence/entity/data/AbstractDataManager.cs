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
namespace org.activiti.engine.impl.persistence.entity.data
{

    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.db;
    using org.activiti.engine.impl.persistence.cache;
    using System.Linq;
    using System.Reflection;

    /// 
    public abstract class AbstractDataManager<EntityImpl> : AbstractManager, IDataManager<EntityImpl> where EntityImpl : IEntity
    {
        public abstract EntityImpl create();

        public abstract Type ManagedEntityClass { get; }

        private static readonly Func<DbSqlSession, Type, Type, KeyValuePair<string, object>, bool, IEntity> selectById = (db, managedType, outType, id, useCache) =>
        {
            MethodInfo m = db.GetType().GetMethod("selectbyid", BindingFlags.Public | BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.IgnoreCase);

            var invoker = m.MakeGenericMethod(new Type[] { managedType, outType });

            var res = invoker.Invoke(db, new object[] { id, useCache });
            if (res == null)
            {
                return default(IEntity);
            }

            return (IEntity)res;
        };

        private static readonly Func<DbSqlSession, Type, Type, string, object, IList<EntityImpl>> selectList = (db, managedType, outType, dbQueryName, parameter) =>
        {
            MethodInfo m = db.GetType().GetMethod("selectList", new Type[] { typeof(string), typeof(object) });

            var invoker = m.MakeGenericMethod(new Type[] { managedType, outType });

            var res = invoker.Invoke(db, new object[] { dbQueryName, parameter });
            if (res == null)
            {
                return default(IList<EntityImpl>);
            }

            return (IList<EntityImpl>)res;
        };

        private static readonly Func<DbSqlSession, Type, Type, string, object, EntityImpl> selectOne = (db, managedType, outType, selectQuery, parameter) =>
        {
            MethodInfo m = db.GetType().GetMethod("selectOne", new Type[] { typeof(string), typeof(object) });

            var invoker = m.MakeGenericMethod(new Type[] { managedType, outType });

            var res = invoker.Invoke(db, new object[] { selectQuery, parameter });
            if (res == null)
            {
                return default(EntityImpl);
            }

            return (EntityImpl)res;
        };

        public AbstractDataManager(ProcessEngineConfigurationImpl processEngineConfiguration) : base(processEngineConfiguration)
        {
        }


        public virtual IList<Type> ManagedEntitySubClasses
        {
            get
            {
                return null;
            }
        }

        protected internal virtual DbSqlSession DbSqlSession
        {
            get
            {
                return getSession<DbSqlSession>();
            }
        }

        protected internal virtual IEntityCache EntityCache
        {
            get
            {
                return getSession<IEntityCache>();
            }
        }

        public virtual TOut findById<TOut>(KeyValuePair<string, object> id)
        {
            if (id.Value == null)
            {
                return default(TOut);
            }

            // Cache
            TOut cachedEntity = (TOut)EntityCache.findInCache(ManagedEntityClass, id.Value?.ToString());
            if (cachedEntity != null)
            {
                return cachedEntity;
            }

            // Database
            return (TOut)selectById(DbSqlSession, ManagedEntityClass, typeof(TOut), id, false);
        }

        public virtual void insert(EntityImpl entity)
        {
            DbSqlSession.insert(entity);
        }

        public virtual EntityImpl update(EntityImpl entity)
        {
            DbSqlSession.update(entity);
            return entity;
        }

        public virtual void delete(KeyValuePair<string, object> entityId)
        {
            EntityImpl entity = findById<EntityImpl>(entityId);

            delete(entity);
        }

        public virtual void delete(EntityImpl entity)
        {
            DbSqlSession.delete(entity);
        }


        protected internal virtual EntityImpl getEntity(string selectQuery, object parameter, ISingleCachedEntityMatcher<EntityImpl> cachedEntityMatcher, bool checkDatabase)
        {
            // Cache
            foreach (EntityImpl cachedEntity in EntityCache.findInCache(ManagedEntityClass))
            {
                if (cachedEntityMatcher.isRetained(cachedEntity, parameter))
                {
                    return cachedEntity;
                }
            }

            // Database
            if (checkDatabase)
            {
                return selectOne(DbSqlSession, ManagedEntityClass, typeof(EntityImpl), selectQuery, parameter);
            }

            return default(EntityImpl);
        }

        /// <summary>
        /// Gets a list by querying the database and the cache using <seealso cref="CachedEntityMatcher"/>.
        /// First, the entities are fetched from the database using the provided query. 
        /// The cache is then queried for the entities of the same type. If an entity matches
        /// the <seealso cref="CachedEntityMatcher"/> condition, it replaces the entity from the database (as it is newer).
        /// </summary>
        /// <param name="dbQueryName"> The query name that needs to be executed. </param>
        /// <param name="parameter"> The parameters for the query. </param>
        /// <param name="entityMatcher"> The matcher used to determine which entities from the cache needs to be retained </param>
        /// <param name="checkCache"> If false, no cache check will be done, and the returned list will simply be the list from the database. </param>
        protected internal virtual ICollection<EntityImpl> getList(string dbQueryName, object parameter, ICachedEntityMatcher<EntityImpl> cachedEntityMatcher, bool checkCache)
        {
            ICollection<EntityImpl> result = selectList(DbSqlSession, ManagedEntityClass, typeof(EntityImpl), dbQueryName, parameter);

            if (checkCache)
            {
                ICollection<CachedEntity> cachedObjects = EntityCache.findInCacheAsCachedObjects(ManagedEntityClass);

                if ((cachedObjects != null && cachedObjects.Count > 0) || ManagedEntitySubClasses != null)
                {
                    Dictionary<string, EntityImpl> entityMap = new Dictionary<string, EntityImpl>(result.Count);

                    // Database entities
                    foreach (EntityImpl entity in result)
                    {
                        entityMap[entity.Id] = entity;
                    }

                    // Cache entities
                    if (cachedObjects != null && cachedEntityMatcher != null)
                    {
                        foreach (CachedEntity cachedObject in cachedObjects)
                        {
                            EntityImpl cachedEntity = (EntityImpl)cachedObject.Entity;
                            if (cachedEntityMatcher.isRetained(result, cachedObjects, cachedEntity, parameter))
                            {
                                entityMap[cachedEntity.Id] = cachedEntity; // will overwite db version with newer version
                            }
                        }
                    }

                    if (ManagedEntitySubClasses != null && cachedEntityMatcher != null)
                    {
                        foreach (Type entitySubClass in ManagedEntitySubClasses)
                        {
                            ICollection<CachedEntity> subclassCachedObjects = EntityCache.findInCacheAsCachedObjects(entitySubClass);
                            if (subclassCachedObjects != null)
                            {
                                foreach (CachedEntity subclassCachedObject in subclassCachedObjects)
                                {
                                    EntityImpl cachedSubclassEntity = (EntityImpl)subclassCachedObject.Entity;
                                    if (cachedEntityMatcher.isRetained(result, cachedObjects, cachedSubclassEntity, parameter))
                                    {
                                        entityMap[cachedSubclassEntity.Id] = cachedSubclassEntity; // will overwite db version with newer version
                                    }
                                }
                            }
                        }
                    }

                    result = entityMap.Values;

                }

            }

            // Remove entries which are already deleted
            if (result.Count > 0)
            {
                var list = new List<EntityImpl>(result);
                for (var idx = list.Count - 1; idx >= 0; idx--)
                {
                    var item = list[idx];
                    if (DbSqlSession.isEntityToBeDeleted(item))
                    {
                        list.RemoveAt(idx);
                    }
                }

                return list;
            }

            return new List<EntityImpl>();
        }

        protected internal virtual IList<EntityImpl> getListFromCache(ICachedEntityMatcher<EntityImpl> entityMatcher, object parameter)
        {
            ICollection<CachedEntity> cachedObjects = EntityCache.findInCacheAsCachedObjects(ManagedEntityClass);

            DbSqlSession dbSqlSession = DbSqlSession;

            IList<EntityImpl> result = new List<EntityImpl>(cachedObjects.Count);
            if (cachedObjects != null && entityMatcher != null)
            {
                foreach (CachedEntity cachedObject in cachedObjects)
                {
                    EntityImpl cachedEntity = (EntityImpl)cachedObject.Entity;
                    if (entityMatcher.isRetained(null, cachedObjects, cachedEntity, parameter) && !dbSqlSession.isEntityToBeDeleted(cachedEntity))
                    {
                        result.Add(cachedEntity);
                    }
                }
            }

            if (ManagedEntitySubClasses != null && entityMatcher != null)
            {
                foreach (Type entitySubClass in ManagedEntitySubClasses)
                {
                    ICollection<CachedEntity> subclassCachedObjects = EntityCache.findInCacheAsCachedObjects(entitySubClass);
                    if (subclassCachedObjects != null)
                    {
                        foreach (CachedEntity subclassCachedObject in subclassCachedObjects)
                        {
                            EntityImpl cachedSubclassEntity = (EntityImpl)subclassCachedObject.Entity;
                            if (entityMatcher.isRetained(null, cachedObjects, cachedSubclassEntity, parameter) && !dbSqlSession.isEntityToBeDeleted(cachedSubclassEntity))
                            {
                                result.Add(cachedSubclassEntity);
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}