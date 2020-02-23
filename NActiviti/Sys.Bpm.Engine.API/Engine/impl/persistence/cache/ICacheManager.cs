using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sys.Core.Cache
{
    /// <summary>
    /// 缓存管理类
    /// </summary>
    public interface ICacheManager
    {
        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <typeparam name="T">泛型缓存实体类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <returns>获取缓存对象</returns>
        T Get<T>(string key);

        /// <summary>
        /// 获取缓存，如果缓存为空，则使用创建工厂创建实例并将实例化对象加入到缓存中
        /// </summary>
        /// <typeparam name="T">泛型缓存实体类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <param name="createFactory">缓存创建工厂</param>
        /// <returns></returns>
        T GetOrCreate<T>(string key, Func<T> createFactory);

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <typeparam name="T">泛型缓存实体类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <param name="item">被缓存对象</param>
        void Set<T>(string key, T item);

        /// <summary>
        /// 设置缓存及缓存相对现在时的过期时间DYA.HOUR:MIN:SEC
        /// </summary>
        /// <typeparam name="T">泛型缓存实体类型/typeparam>
        /// <param name="key">缓存键</param>
        /// <param name="item">被缓存对象</param>
        /// <param name="absoluteExpiration"></param>
        void Set<T>(string key, T item, TimeSpan absoluteExpiration);

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="key">缓存键</param>
        void Remove(string key);

        /// <summary>
        /// 是否包含缓存
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns>true：包含 false：不包含</returns>
        bool Contains(string key);

        /// <summary>
        /// 清空缓存
        /// </summary>
        void Clear();
    }
}
