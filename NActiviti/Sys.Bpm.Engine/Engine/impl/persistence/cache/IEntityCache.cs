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

    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using System.Collections.Concurrent;

    /// <summary>
    /// This is a cache for <seealso cref="IEntity"/> instances during the execution of one <seealso cref="Command"/>.
    /// 
    /// 
    /// </summary>
    public interface IEntityCache : ISession
    {
        /// <summary>
        /// Returns all cached <seealso cref="IEntity"/> instances as a map 
        /// with following structure: { entityClassName, {entityId, entity} }
        /// </summary>
        IDictionary<Type, IDictionary<string, CachedEntity>> AllCachedEntities { get; }

        /// <summary>
        /// Adds the gives <seealso cref="IEntity"/> to the cache.
        /// </summary>
        /// <param name="entity"> The <seealso cref="IEntity"/> instance </param>
        /// <param name="storeState"> If true, the current state <seealso cref="IEntity#getPersistentState()"/> will be stored for future diffing.
        ///                   Note that, if false, the <seealso cref="IEntity"/> will always be seen as changed. </param>
        /// <returns> Returns a <seealso cref="CachedEntity"/> instance, which can be enriched later on.                    </returns>
        CachedEntity Put(IEntity entity, bool storeState);

        /// <summary>
        /// Returns the cached <seealso cref="IEntity"/> instance of the given class with the provided id.
        /// Returns null if such a <seealso cref="IEntity"/> cannot be found. 
        /// </summary>
        T FindInCache<T>(string id);

        object FindInCache(Type cacheType, string id);

        /// <summary>
        /// Returns all cached <seealso cref="IEntity"/> instances of a given type.
        /// Returns an empty list if no instances of the given type exist.
        /// </summary>
        IList<T> FindInCache<T>();

        IList<object> FindInCache(Type cacheType);

        /// <summary>
        /// Returns all <seealso cref="CachedEntity"/> instances for the given type.
        /// The difference with <seealso cref="#findInCache(Class)"/> is that here the whole <seealso cref="CachedEntity"/>
        /// is returned, which gives access to the persistent state at the moment of putting it in the cache.  
        /// </summary>
        ICollection<CachedEntity> FindInCacheAsCachedObjects<T>();

        ICollection<CachedEntity> FindInCacheAsCachedObjects(Type entityClass);

        /// <summary>
        /// Removes the <seealso cref="IEntity"/> of the given type with the given id from the cache. 
        /// </summary>
        void CacheRemove(Type entityClass, string entityId);
    }
}