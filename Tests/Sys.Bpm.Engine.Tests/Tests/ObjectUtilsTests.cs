using SmartSql.Utils;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using Xunit;

namespace Sys.Bpm.Engine.Tests.Tests
{
    public class ObjectUtilsTests
    {
        private sealed class SampleRefType
        {
            public SampleRefType(int intValue, string textValue)
            {
                IntValue = intValue;
                TextValue = textValue;
                CreatedAt = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
                RefValue = new object();
            }

            public int IntValue { get; }

            public string TextValue { get; }

            public DateTime CreatedAt { get; }

            public object RefValue { get; }
        }

        private sealed class WithIndexerAndStatic
        {
            public int X => 2;

            public static int StaticValue => 3;

            public string this[int index] => index.ToString();
        }

        private sealed class WithByRefLike
        {
            public int X => 2;

            public ReadOnlySpan<byte> Data => new byte[] { 1, 2, 3 };
        }

        private class WithNonPublicGetters
        {
            public WithNonPublicGetters()
            {
                PublicValue = 1;
                InternalValue = 2;
                ProtectedValue = 3;
                PrivateValue = 4;
            }

            public int PublicValue { get; }

            internal int InternalValue { get; }

            protected int ProtectedValue { get; }

            private int PrivateValue { get; }
        }

        private readonly struct SampleValueType
        {
            public SampleValueType(int a, int b)
            {
                A = a;
                B = b;
            }

            public int A { get; }

            public int B { get; }
        }

        private sealed class NullableKeyDictionary : IDictionary
        {
            private readonly List<DictionaryEntry> _entries = new List<DictionaryEntry>();

            public object this[object key]
            {
                get
                {
                    foreach (var entry in _entries)
                    {
                        if (KeysEqual(entry.Key, key))
                        {
                            return entry.Value;
                        }
                    }

                    return null;
                }
                set
                {
                    Add(key, value);
                }
            }

            public ICollection Keys
            {
                get
                {
                    var keys = new ArrayList(_entries.Count);
                    foreach (var entry in _entries)
                    {
                        keys.Add(entry.Key);
                    }
                    return keys;
                }
            }

            public ICollection Values
            {
                get
                {
                    var values = new ArrayList(_entries.Count);
                    foreach (var entry in _entries)
                    {
                        values.Add(entry.Value);
                    }
                    return values;
                }
            }

            public bool IsReadOnly => false;

            public bool IsFixedSize => false;

            public int Count => _entries.Count;

            public object SyncRoot { get; } = new object();

            public bool IsSynchronized => false;

            public void Add(object key, object value)
            {
                _entries.Add(new DictionaryEntry(key, value));
            }

            public void Clear()
            {
                _entries.Clear();
            }

            public bool Contains(object key)
            {
                foreach (var entry in _entries)
                {
                    if (KeysEqual(entry.Key, key))
                    {
                        return true;
                    }
                }
                return false;
            }

            public IDictionaryEnumerator GetEnumerator()
            {
                return new DictionaryEntryEnumerator(_entries);
            }

            public void Remove(object key)
            {
                for (int i = 0; i < _entries.Count; i++)
                {
                    if (KeysEqual(_entries[i].Key, key))
                    {
                        _entries.RemoveAt(i);
                        return;
                    }
                }
            }

            public void CopyTo(Array array, int index)
            {
                for (int i = 0; i < _entries.Count; i++)
                {
                    array.SetValue(_entries[i], index + i);
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _entries.GetEnumerator();
            }

            private static bool KeysEqual(object left, object right)
            {
                if (left is null && right is null) return true;
                if (left is null || right is null) return false;
                return left.Equals(right);
            }

            private sealed class DictionaryEntryEnumerator : IDictionaryEnumerator
            {
                private readonly IList<DictionaryEntry> _entries;
                private int _index = -1;

                public DictionaryEntryEnumerator(IList<DictionaryEntry> entries)
                {
                    _entries = entries;
                }

                public object Current => Entry;

                public DictionaryEntry Entry => _entries[_index];

                public object Key => _entries[_index].Key;

                public object Value => _entries[_index].Value;

                public bool MoveNext()
                {
                    _index++;
                    return _index < _entries.Count;
                }

                public void Reset()
                {
                    _index = -1;
                }
            }
        }

        /// <summary>
        /// 验证 null 输入时返回空字典，不抛异常。
        /// </summary>
        [Fact]
        public void ToDictionary_Null_ReturnsEmptyDictionary()
        {
            var dict1 = ObjectUtils.ToDictionary(null, ignorePropNameCase: true);
            Assert.NotNull(dict1);
            Assert.Empty(dict1);

            var dict2 = ObjectUtils.ToDictionary(null, ignorePropNameCase: false);
            Assert.NotNull(dict2);
            Assert.Empty(dict2);
        }

        /// <summary>
        /// 验证 IDictionary 分支在忽略大小写时会合并键（后写覆盖前写）。
        /// </summary>
        [Fact]
        public void ToDictionary_IDictionary_IgnoreCaseTrue_CollapsesKeys()
        {
            IDictionary ht = new OrderedDictionary
            {
                ["a"] = 1,
                ["A"] = 2
            };

            var dict = ObjectUtils.ToDictionary(ht, ignorePropNameCase: true);

            Assert.Single(dict);
            Assert.Equal(2, dict["a"]);
            Assert.Equal(2, dict["A"]);
        }

        /// <summary>
        /// 验证 IDictionary 分支在区分大小写时不会合并键。
        /// </summary>
        [Fact]
        public void ToDictionary_IDictionary_IgnoreCaseFalse_KeepDistinctKeys()
        {
            IDictionary ht = new Hashtable
            {
                ["a"] = 1,
                ["A"] = 2
            };

            var dict = ObjectUtils.ToDictionary(ht, ignorePropNameCase: false);

            Assert.Equal(2, dict.Count);
            Assert.Equal(1, dict["a"]);
            Assert.Equal(2, dict["A"]);
        }

        /// <summary>
        /// 验证 IDictionary 分支对 null key 会抛出明确异常。
        /// </summary>
        [Fact]
        public void ToDictionary_IDictionary_NullKey_Throws()
        {
            IDictionary dict = new NullableKeyDictionary();
            dict.Add(null, 1);

            Assert.Throws<ArgumentException>(() => ObjectUtils.ToDictionary(dict, ignorePropNameCase: true));
        }

        /// <summary>
        /// 验证 IDictionary 分支对空白 key 会抛出明确异常。
        /// </summary>
        [Fact]
        public void ToDictionary_IDictionary_EmptyKey_Throws()
        {
            IDictionary ht = new Hashtable
            {
                [""] = 1
            };

            Assert.Throws<ArgumentException>(() => ObjectUtils.ToDictionary(ht, ignorePropNameCase: true));
        }

        /// <summary>
        /// 验证反射分支能正确映射引用类型的可读实例属性。
        /// </summary>
        [Fact]
        public void ToDictionary_Object_MapsReadableInstanceProperties()
        {
            var obj = new SampleRefType(10, "hello");

            var dict = ObjectUtils.ToDictionary(obj, ignorePropNameCase: true);

            Assert.Equal(4, dict.Count);
            Assert.Equal(10, dict[nameof(SampleRefType.IntValue)]);
            Assert.Equal("hello", dict[nameof(SampleRefType.TextValue)]);
            Assert.Equal(new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc), dict[nameof(SampleRefType.CreatedAt)]);
            Assert.Same(obj.RefValue, dict[nameof(SampleRefType.RefValue)]);
        }

        /// <summary>
        /// 验证反射分支会忽略 indexer 与 static 属性。
        /// </summary>
        [Fact]
        public void ToDictionary_Object_IgnoresIndexerAndStaticProperties()
        {
            var obj = new WithIndexerAndStatic();

            var dict = ObjectUtils.ToDictionary(obj, ignorePropNameCase: true);

            Assert.Single(dict);
            Assert.Equal(2, dict["X"]);
            Assert.False(dict.ContainsKey("StaticValue"));
            Assert.False(dict.ContainsKey("Item"));
        }

        /// <summary>
        /// 验证反射分支能正确映射值类型的可读实例属性。
        /// </summary>
        [Fact]
        public void ToDictionary_Struct_MapsReadableInstanceProperties()
        {
            var obj = new SampleValueType(1, 2);

            var dict = ObjectUtils.ToDictionary(obj, ignorePropNameCase: true);

            Assert.Equal(2, dict.Count);
            Assert.Equal(1, dict["A"]);
            Assert.Equal(2, dict["B"]);
        }

        /// <summary>
        /// 验证反射分支会跳过 byref-like 属性（例如 ReadOnlySpan）。
        /// </summary>
        [Fact]
        public void ToDictionary_Object_IgnoresByRefLikeProperties()
        {
            var obj = new WithByRefLike();

            var dict = ObjectUtils.ToDictionary(obj, ignorePropNameCase: true);

            Assert.Single(dict);
            Assert.Equal(2, dict["X"]);
            Assert.False(dict.ContainsKey("Data"));
        }

        /// <summary>
        /// 验证支持 internal/private/protected getter（非 public）被转换为参数。
        /// </summary>
        [Fact]
        public void ToDictionary_Object_IncludesNonPublicGetters()
        {
            var obj = new WithNonPublicGetters();

            var dict = ObjectUtils.ToDictionary(obj, ignorePropNameCase: true);

            Assert.Equal(4, dict.Count);
            Assert.Equal(1, dict["PublicValue"]);
            Assert.Equal(2, dict["InternalValue"]);
            Assert.Equal(3, dict["ProtectedValue"]);
            Assert.Equal(4, dict["PrivateValue"]);
        }

        /// <summary>
        /// 验证缓存键不会因匿名类型而碰撞，避免错误的 converter 被复用导致 InvalidCastException。
        /// </summary>
        [Fact]
        public void ToDictionary_CacheKey_DoesNotCollideAcrossAnonymousTypes()
        {
            var list = new List<int> { 1, 2, 3 };

            object obj1 = new { Items = (IEnumerable<int>)list };
            object obj2 = new { Items = (IList<int>)list };

            var dict1 = ObjectUtils.ToDictionary(obj1, ignorePropNameCase: true);
            var dict2 = ObjectUtils.ToDictionary(obj2, ignorePropNameCase: true);
            var dict3 = ObjectUtils.ToDictionary(obj1, ignorePropNameCase: true);

            Assert.Same(list, dict1["Items"]);
            Assert.Same(list, dict2["Items"]);
            Assert.Same(list, dict3["Items"]);
        }
    }
}
