using Microsoft.Extensions.Caching.Memory;
using Sys.Workflow;
using Sys.Workflow.Caches;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

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
        // volatile：Clear 采用原子替换发布新实例，保证并发读能及时看到
        private volatile IMemoryCache cache = null;
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
            // 配置了上限（SizeLimit>0）时 MemoryCache 强制要求每个缓存项声明 Size，否则抛 InvalidOperationException；
            // 每个条目按 1 计数，此时上限即最大缓存条目数；无上限时该 Size 不参与统计，无副作用
            _ = cache.Set(id, obj, new MemoryCacheEntryOptions { Size = 1 });
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
            // 原子替换：先构造新实例再一次性换掉引用，避免并发读写拿到已 Dispose 的旧实例；
            // 旧实例可能有并发读仍在使用，不主动 Dispose，交由 GC 回收
            _ = Interlocked.Exchange(ref cache, memoryCacheProvider.Create(sizeLimit));
        }
    }
}