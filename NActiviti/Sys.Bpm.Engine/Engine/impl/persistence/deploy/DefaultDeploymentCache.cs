using Microsoft.Extensions.Caching.Memory;
using Sys.Workflow;
using Sys.Workflow.Caches;
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
namespace Sys.Workflow.Engine.Impl.Persistence.Deploies
{
    /// <summary>
    /// Default cache: keep everything in memory, unless a limit is set.
    /// 
    /// 
    /// </summary>
    public class DefaultDeploymentCache<T> : IDeploymentCache<T>
    {
        private IMemoryCache cache = null;
        private readonly MemoryCacheProvider memoryCacheProvider;
        private readonly int sizeLimit = -1;

        /// <summary>
        /// Cache with no limit </summary>
        public DefaultDeploymentCache() : this(-1)
        {

        }

        /// <summary>
        /// Cache which has a hard limit: no more elements will be cached than the limit.
        /// </summary>
        public DefaultDeploymentCache(int limit)
        {
            this.sizeLimit = limit;

            memoryCacheProvider = ProcessEngineServiceProvider.Resolve<MemoryCacheProvider>();

            cache = memoryCacheProvider.Create(limit);
        }

        public virtual T Get(string id)
        {
            cache.TryGetValue(id, out T obj);

            return obj;
        }

        public virtual void Add(string id, T obj)
        {
            _ = cache.Set(id, obj);
        }

        public virtual void Remove(string id)
        {
            if (id is null)
            {
                return;
            }
            cache.Remove(id);
        }

        public virtual bool Contains(string id)
        {
            return cache.TryGetValue(id, out _);
        }

        public virtual void Clear()
        {
            cache.Dispose();
            cache = memoryCacheProvider.Create(sizeLimit);
        }
    }
}