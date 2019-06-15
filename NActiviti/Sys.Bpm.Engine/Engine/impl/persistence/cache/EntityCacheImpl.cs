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
namespace org.activiti.engine.impl.persistence.cache
{
    using org.activiti.engine.impl.persistence.entity;
    using System.Collections.Concurrent;
    using System.Linq;

    /// 
    public class EntityCacheImpl : IEntityCache
    {
        protected internal readonly ConcurrentDictionary<Type, IDictionary<string, CachedEntity>> cachedObjects = new ConcurrentDictionary<Type, IDictionary<string, CachedEntity>>();

        public virtual CachedEntity Put(IEntity entity, bool storeState)
        {
            ConcurrentDictionary<string, CachedEntity> classCache = cachedObjects.GetOrAdd(entity.GetType(), new ConcurrentDictionary<string, CachedEntity>()) as ConcurrentDictionary<string, CachedEntity>;
            CachedEntity cachedObject = new CachedEntity(entity, storeState);
            classCache.AddOrUpdate(entity.Id, cachedObject, (id, cacheObject) => cachedObject);
            return cachedObject;
        }

        public virtual T FindInCache<T>(string id)
        {
            Type entityClass = typeof(T);

            return (T)FindInCache(entityClass, id);
        }

        public virtual object FindInCache(Type entityClass, string id)
        {
            CachedEntity cachedObject = null;
            cachedObjects.TryGetValue(entityClass, out IDictionary<string, CachedEntity> classCache);
            if (classCache == null)
            {
                classCache = FindClassCacheByCheckingSubclasses(entityClass);
            }

            if (classCache != null)
            {
                classCache.TryGetValue(id, out cachedObject);
            }

            if (cachedObject != null)
            {
                return cachedObject.Entity;
            }

            return null;
        }

        protected internal virtual IDictionary<string, CachedEntity> FindClassCacheByCheckingSubclasses(Type entityClass)
        {
            foreach (Type clazz in cachedObjects.Keys)
            {
                if (entityClass.IsAssignableFrom(clazz))
                {
                    cachedObjects.TryGetValue(clazz, out IDictionary<string, CachedEntity> ret);

                    return ret;
                }
            }
            return null;
        }

        public virtual void CacheRemove(Type entityClass, string entityId)
        {
            cachedObjects.TryGetValue(entityClass, out IDictionary<string, CachedEntity> classCache);
            if (classCache is ConcurrentDictionary<string, CachedEntity> dict)
            {
                dict.TryRemove(entityId, out _);
            }
        }

        public virtual ICollection<CachedEntity> FindInCacheAsCachedObjects<T>()
        {
            Type entityClass = typeof(T);

            return FindInCacheAsCachedObjects(entityClass);
        }

        public virtual ICollection<CachedEntity> FindInCacheAsCachedObjects(Type entityClass)
        {
            cachedObjects.TryGetValue(entityClass, out IDictionary<string, CachedEntity> classCache);
            if (classCache is ConcurrentDictionary<string, CachedEntity> dict)
            {
                return dict.Values;
            }

            return null;
        }

        public virtual IList<T> FindInCache<T>()
        {
            Type entityClass = typeof(T);

            var ret = FindInCache(entityClass);
            if (ret != null)
            {
                return ret.Select(x => (T)x).ToList();
            }

            return null;
        }

        public virtual IList<object> FindInCache(Type entityClass)
        {
            cachedObjects.TryGetValue(entityClass, out IDictionary<string, CachedEntity> classCache);

            if (classCache is null)
            {
                classCache = FindClassCacheByCheckingSubclasses(entityClass);
            }

            if (!(classCache is null))
            {
                IList<object> entities = new List<object>(classCache.Count);
                foreach (CachedEntity cachedObject in classCache.Values)
                {
                    entities.Add(cachedObject.Entity);
                }
                return entities;
            }

            return new List<object>();
        }

        public virtual IDictionary<Type, IDictionary<string, CachedEntity>> AllCachedEntities
        {
            get
            {
                return cachedObjects;
            }
        }

        public virtual void Close()
        {

        }

        public virtual void Flush()
        {

        }
    }
}