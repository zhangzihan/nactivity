using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Sys.Workflow.Engine.Api
{
    /// <summary>
    /// 
    /// </summary>
    public static class UrlUtil
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseAddress"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string Fixed(string baseAddress, string url)
        {
            if (string.IsNullOrWhiteSpace(baseAddress))
            {
                return url;
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                return baseAddress;
            }

            var reg = new Regex("\\s+(http|https)://");
            if (reg.IsMatch(url))
            {
                return url;
            }

            return (baseAddress.EndsWith("/") ? baseAddress : baseAddress + "/") + (url.StartsWith("/") ? url.Remove(0, 1) : url);
        }
    }
}
