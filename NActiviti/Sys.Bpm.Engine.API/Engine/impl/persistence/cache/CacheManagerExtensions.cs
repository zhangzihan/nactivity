using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sys.Core.Cache
{
    /// <summary>
    /// 
    /// </summary>
    public static class CacheManagerExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configSectionName"></param>
        /// <returns></returns>
        public static IServiceCollection AddCache(this IServiceCollection services, string configSectionName = "CacheOptions")
        {
            IConfiguration configuration = services.FirstOrDefault(x => typeof(IConfiguration) == x.ServiceType).ImplementationInstance as IConfiguration;

            IConfiguration cacheConfig = configuration.GetSection(configSectionName);

            string typeName = cacheConfig.GetValue<string>("Type");

            services.PostConfigure<InMemoryCacheOptions>(opts =>
            {
                var inMem = cacheConfig.GetSection("Options:InMemory");
                if (inMem != null)
                {
                    inMem.Bind(opts);
                }
            });

            services.AddMemoryCache(opts =>
            {
                //var inMem = cacheConfig.GetSection("Options:InMemory");
                //var limit = inMem?.GetValue<long?>("SizeLimit");
                //opts.SizeLimit = limit.HasValue ? limit.Value == -1 ? long.MaxValue : limit.Value : long.MaxValue;
            });

            services.AddSingleton(sp =>
            {
                Type type = Type.GetType(typeName);
                if (typeof(IMemoryCacheManager).IsAssignableFrom(type))
                {
                    IMemoryCache memoryCache = sp.GetService<IMemoryCache>();
                    var options = sp.GetService<IOptions<InMemoryCacheOptions>>();
                    return Activator.CreateInstance(type, memoryCache, options) as ICacheManager;
                }
                else if (typeof(IDistributedCacheManager).IsAssignableFrom(type))
                {
                    return Activator.CreateInstance(type, sp.GetService<DistributedCacheOptions>()) as ICacheManager;
                }

                return null;
            });

            return services;
        }
    }
}
