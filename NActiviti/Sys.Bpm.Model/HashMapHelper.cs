//---------------------------------------------------------------------------------------------------------
//	Copyright © 2007 - 2018 Tangible Software Solutions Inc.
//	This class can be used by anyone provided that the copyright notice remains intact.
//
//	This class is used to replace calls to some Java HashMap or Hashtable methods.
//	DEPRECATED: Use C# native Dictionary and LINQ methods instead.
//---------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;

public static class HashMapHelper
{
    [Obsolete("Use dictionary.ToHashSet() or new HashSet<KeyValuePair<TKey, TValue>>(dictionary) instead.")]
    public static HashSet<KeyValuePair<TKey, TValue>> SetOfKeyValuePairs<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
    {
        return new HashSet<KeyValuePair<TKey, TValue>>(dictionary);
    }

    [Obsolete("Use dictionary.TryGetValue(key, out var value) ? value : default instead.")]
    internal static TValue GetValueOrNull<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
    {
        dictionary.TryGetValue(key, out TValue ret);
        return ret;
    }
}
