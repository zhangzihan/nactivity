using System.Collections.Generic;

namespace Sys.Workflow
{
    public static class DictionaryHelper
    {
        public static IDictionary<TKey, TValue> PutAll<TKey, TValue>(this IDictionary<TKey, TValue> target, IDictionary<TKey, TValue> source)
        {
            if (source is object)
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
    }
}
