using SmartSql.Abstractions.Cache;
using SmartSql.Cache;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace SmartSql.Configuration
{
    public class CacheFactory
    {
        public static Cache Load(XElement cacheNode)
        {
            var cache = new Cache
            {
                Id = cacheNode.Attribute("Id").Value,
                Type = cacheNode.Attribute("Type").Value,
                Parameters = new Dictionary<String, String>(),
                FlushOnExecutes = new List<FlushOnExecute>()
            };
            foreach (XElement childNode in cacheNode.Elements())
            {
                switch (childNode.Name.LocalName)
                {
                    case "Parameter":
                        {
                            string key = childNode.Attribute("Key")?.Value;
                            string val = childNode.Attribute("Value")?.Value;
                            if (!String.IsNullOrEmpty(key))
                            {
                                cache.Parameters.Add(key, val);
                            }
                            break;
                        }
                    case "FlushInterval":
                        {
                            string hours = childNode.Attribute("Hours")?.Value;
                            string minutes = childNode.Attribute("Minutes")?.Value;
                            string seconds = childNode.Attribute("Seconds")?.Value;
                            cache.FlushInterval = new FlushInterval
                            {
                                Hours = XmlConvert.ToInt32(hours),
                                Minutes = XmlConvert.ToInt32(minutes),
                                Seconds = XmlConvert.ToInt32(seconds)
                            };
                            break;
                        }
                    case "FlushOnExecute":
                        {
                            string statementId = childNode.Attribute("Statement")?.Value;
                            if (!String.IsNullOrEmpty(statementId))
                            {
                                cache.FlushOnExecutes.Add(new FlushOnExecute
                                {
                                    Statement = statementId
                                });
                            }
                            break;
                        }
                }
            }
            cache.Provider = CreateCacheProvider(cache);
            return cache;
        }
        private static ICacheProvider CreateCacheProvider(Cache cache)
        {
            ICacheProvider _cacheProvider = null;
            switch (cache.Type)
            {
                case "Lru":
                    {
                        _cacheProvider = new LruCacheProvider();
                        break;
                    }
                case "Fifo":
                    {
                        _cacheProvider = new FifoCacheProvider();
                        break;
                    }
                default:
                    {
                        var assName = new AssemblyName { Name = cache.AssemblyName };
                        Type _cacheProviderType = Assembly.Load(assName).GetType(cache.TypeName);
                        _cacheProvider = Activator.CreateInstance(_cacheProviderType) as ICacheProvider;
                        break;
                    }
            }
            _cacheProvider.Initialize(cache.Parameters);
            return _cacheProvider;
        }
    }
}
