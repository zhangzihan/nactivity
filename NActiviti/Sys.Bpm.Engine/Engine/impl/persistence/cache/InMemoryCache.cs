using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sys.Core.Cache
{
    /// <summary>
    /// 使用内存CACHE
    /// </summary>
    public class InMemoryCache : IMemoryCacheManager
    {
        private readonly IMemoryCache cache;
        private readonly InMemoryCacheOptions options;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="cache">实现IMemoryCache接口的实现类</param>
        /// <param name="options" cref="InMemoryCache">配置信息</param>
        public InMemoryCache(IMemoryCache cache,
            IOptions<InMemoryCacheOptions> options)
        {
            this.cache = cache ?? throw new ArgumentNullException("IMemoryCache can not be null!");
            this.options = options.Value;
        }

        /// <inheritdoc />
        public void Clear()
        {
            cache.Dispose();
        }

        /// <inheritdoc />
        public bool Contains(string key)
        {
            return cache.TryGetValue(key, out _);
        }

        /// <inheritdoc />
        public T Get<T>(string key)
        {
            if (cache.TryGetValue(key, out T item))
            {
                return item;
            }

            return default;
        }

        /// <inheritdoc />
        public void Remove(string key)
        {
            cache.Remove(key);
        }

        /// <inheritdoc />
        public void Set<T>(string key, T item)
        {
            var exp = options.AbsoluteExpiration;
            Set(key, item, exp.HasValue ? exp.Value.Days < 0 ? TimeSpan.FromDays(365) : exp.Value : TimeSpan.FromDays(365));
        }

        /// <inheritdoc />
        public void Set<T>(string key, T item, TimeSpan absoluteExpiration)
        {
            cache.Set(key, item, absoluteExpiration);
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        /// <inheritdoc />
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    cache?.Dispose();
                }

                disposedValue = true;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
        }

        /// <inheritdoc />
        public T GetOrCreate<T>(string key, Func<T> createFactory)
        {
            if (createFactory is null)
            {
                throw new ArgumentNullException(nameof(createFactory));
            }

            T obj = createFactory();

            return obj;
        }
        #endregion
    }
}
