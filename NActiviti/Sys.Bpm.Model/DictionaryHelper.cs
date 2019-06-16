using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Sys.Workflow
{
    public static class DictionaryHelper
    {
        public static IDictionary<TKey, TValue> PutAll<TKey, TValue>(this IDictionary<TKey, TValue> target, IDictionary<TKey, TValue> source)
        {
            if (source != null)
            {
                foreach (var key in source.Keys)
                {
                    if (!target.ContainsKey(key))
                    {
                        target.Add(key, source[key]);
                    }
                }
            }

            return target;
        }

        public static ConcurrentDictionary<TKey, TValue> PutAll<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> target, ConcurrentDictionary<TKey, TValue> source)
        {
            if (source != null)
            {
                foreach (var key in source.Keys)
                {
                    if (!target.ContainsKey(key))
                    {
                        source.TryGetValue(key, out var value);
                        target.TryAdd(key, value);
                    }
                }
            }

            return target;
        }
    }
}
