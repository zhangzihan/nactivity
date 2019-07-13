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
        private static readonly Regex URL_PATTERN = new Regex("\\s+(http|https)://", RegexOptions.Compiled);

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

            if (URL_PATTERN.IsMatch(url))
            {
                return url;
            }

            return (baseAddress.EndsWith("/") ? baseAddress : baseAddress + "/") + (url.StartsWith("/") ? url.Remove(0, 1) : url);
        }
    }
}
