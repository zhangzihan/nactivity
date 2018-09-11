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

        protected internal IDictionary<Type, IDictionary<string, CachedEntity>> cachedObjects = new Dictionary<Type, IDictionary<string, CachedEntity>>();

        public virtual CachedEntity put(IEntity entity, bool storeState)
        {
            cachedObjects.TryGetValue(entity.GetType(), out var classCache);
            if (classCache == null)
            {
                classCache = new Dictionary<string, CachedEntity>();
                cachedObjects[entity.GetType()] = classCache;
            }
            CachedEntity cachedObject = new CachedEntity(entity, storeState);
            classCache[entity.Id] = cachedObject;
            return cachedObject;
        }

        public virtual T findInCache<T>(string id)
        {
            Type entityClass = typeof(T);

            return (T)findInCache(entityClass, id);
        }

        public virtual object findInCache(Type entityClass, string id)
        {
            CachedEntity cachedObject = null;
            cachedObjects.TryGetValue(entityClass, out IDictionary<string, CachedEntity> classCache);
            if (classCache == null)
            {
                classCache = findClassCacheByCheckingSubclasses(entityClass);
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

        protected internal virtual IDictionary<string, CachedEntity> findClassCacheByCheckingSubclasses(Type entityClass)
        {
            foreach (Type clazz in cachedObjects.Keys)
            {
                if (entityClass.IsAssignableFrom(clazz))
                {
                    return cachedObjects[clazz];
                }
            }
            return null;
        }

        public virtual void cacheRemove(Type entityClass, string entityId)
        {
            cachedObjects.TryGetValue(entityClass, out IDictionary<string, CachedEntity> classCache);
            if (classCache == null)
            {
                return;
            }
            classCache.Remove(entityId);
        }

        public virtual ICollection<CachedEntity> findInCacheAsCachedObjects<T>()
        {
            Type entityClass = typeof(T);

            return findInCacheAsCachedObjects(entityClass);
        }

        public virtual ICollection<CachedEntity> findInCacheAsCachedObjects(Type entityClass)
        {
            cachedObjects.TryGetValue(entityClass, out IDictionary<string, CachedEntity> classCache);
            if (classCache != null)
            {
                return classCache.Values;
            }
            return null;
        }

        public virtual IList<T> findInCache<T>()
        {
            Type entityClass = typeof(T);

            var ret = findInCache(entityClass);
            if (ret != null)
            {
                return ret.Select(x => (T)x).ToList();
            }

            return null;
        }

        public virtual IList<object> findInCache(Type entityClass)
        {
            cachedObjects.TryGetValue(entityClass, out IDictionary<string, CachedEntity> classCache);

            if (classCache == null)
            {
                classCache = findClassCacheByCheckingSubclasses(entityClass);
            }

            if (classCache != null)
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

        public virtual void close()
        {

        }

        public virtual void flush()
        {

        }
    }
}