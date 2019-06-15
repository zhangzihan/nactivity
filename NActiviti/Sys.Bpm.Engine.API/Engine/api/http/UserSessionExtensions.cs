using Microsoft.Extensions.DependencyInjection;
using Sys.Net.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sys.Extentions
{
    /// <summary>
    /// HttpClient Accesstoken
    /// </summary>
    public static class UserSessionExtensions
    {
        /// <summary>
        /// 注入accesstokenprovider
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddUserSession<T>(this IServiceCollection services) where T : class, IUserSession
        {
            var sp = services.FirstOrDefault(x => x.ServiceType == typeof(IUserSession));
            if (sp != null)
            {
                services.Remove(sp);
            }

            services.AddScoped<IUserSession, T>();

            return services;
        }
    }
}
