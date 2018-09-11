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
namespace org.activiti.engine.impl.persistence.deploy
{
    /// <summary>
    /// Default cache: keep everything in memory, unless a limit is set.
    /// 
    /// 
    /// </summary>
    public class DefaultDeploymentCache<T> : IDeploymentCache<T>
    {
        protected internal IDictionary<string, T> cache;

        /// <summary>
        /// Cache with no limit </summary>
        public DefaultDeploymentCache()
        {
            this.cache = new Dictionary<string, T>();
        }

        /// <summary>
        /// Cache which has a hard limit: no more elements will be cached than the limit.
        /// </summary>
        public DefaultDeploymentCache(int limit)
        {
            this.cache = new LinkedHashMapAnonymousInnerClass(this, limit + 1);
        }

        private class LinkedHashMapAnonymousInnerClass : Dictionary<string, T>
        {
            private readonly DefaultDeploymentCache<T> outerInstance;

            private int limit;

            // 0.75 is the default (see javadocs)
            // true will keep the 'access-order', which is needed to have a real LRU cache
            private long serialVersionUID;

            public LinkedHashMapAnonymousInnerClass(DefaultDeploymentCache<T> outerInstance, int limit) : base(limit, StringComparer.OrdinalIgnoreCase)
            {
                this.outerInstance = outerInstance;
                this.limit = limit;
                serialVersionUID = 1L;
            }

            protected internal virtual bool removeEldestEntry(KeyValuePair<string, T> eldest)
            {
                bool removeEldest = outerInstance.size() > limit;
                //if (removeEldest && logger.TraceEnabled)
                //{
                //    logger.trace("Cache limit is reached, {} will be evicted", eldest.Key);
                //}
                return removeEldest;
            }

        }

        public virtual T get(string id)
        {
            cache.TryGetValue(id, out T obj);

            return obj;
        }

        public virtual void add(string id, T obj)
        {
            cache[id] = obj;
        }

        public virtual void remove(string id)
        {
            if (id == null)
            {
                return;
            }
            if (contains(id))
            {
                cache.Remove(id);
            }
        }

        public virtual bool contains(string id)
        {
            return cache.ContainsKey(id);
        }

        public virtual void clear()
        {
            cache.Clear();
        }

        // For testing purposes only
        public virtual int size()
        {
            return cache.Count;
        }

    }

}