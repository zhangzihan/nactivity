using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sys.Rules.DmnEngine.Feel.Types
{
    /// <summary>
    /// FEEL context type 鈥?an ordered dictionary preserving insertion order.
    /// Keys are case-sensitive strings (FEEL names). Values can be any FEEL-compatible object.
    /// </summary>
    public sealed class FeelContext : IEnumerable<KeyValuePair<string, object>>
    {
        private readonly List<KeyValuePair<string, object>> _entries = new();
        private readonly Dictionary<string, int> _index = new(StringComparer.Ordinal);

        /// <summary>
        /// Number of entries in the context
        /// </summary>
        public int Count => _entries.Count;

        /// <summary>
        /// Gets or sets a value by key. Setting a key that exists overwrites it in place;
        /// setting a new key appends it.
        /// </summary>
        public object this[string key]
        {
            get
            {
                if (_index.TryGetValue(key, out var idx))
                    return _entries[idx].Value;
                return null; // FEEL returns null for missing keys
            }
            set
            {
                if (_index.TryGetValue(key, out var idx))
                {
                    _entries[idx] = new KeyValuePair<string, object>(key, value);
                }
                else
                {
                    _index[key] = _entries.Count;
                    _entries.Add(new KeyValuePair<string, object>(key, value));
                }
            }
        }

        /// <summary>
        /// Checks if the context contains the given key
        /// </summary>
        public bool ContainsKey(string key) => _index.ContainsKey(key);

        /// <summary>
        /// Tries to get the value for the given key
        /// </summary>
        public bool TryGetValue(string key, out object value)
        {
            if (_index.TryGetValue(key, out var idx))
            {
                value = _entries[idx].Value;
                return true;
            }
            value = null;
            return false;
        }

        /// <summary>
        /// Gets the ordered list of keys
        /// </summary>
        public IReadOnlyList<string> Keys => _entries.Select(e => e.Key).ToList();

        /// <summary>
        /// Gets the ordered list of values
        /// </summary>
        public IReadOnlyList<object> Values => _entries.Select(e => e.Value).ToList();

        /// <summary>
        /// Gets the ordered list of entries
        /// </summary>
        public IReadOnlyList<KeyValuePair<string, object>> Entries => _entries.AsReadOnly();

        /// <summary>
        /// Adds a key-value pair. If the key already exists, its value is overwritten.
        /// </summary>
        public void Put(string key, object value) => this[key] = value;

        /// <summary>
        /// Removes a key and its value. Returns true if the key was found.
        /// </summary>
        public bool Remove(string key)
        {
            if (!_index.TryGetValue(key, out var idx))
                return false;

            _entries.RemoveAt(idx);
            _index.Remove(key);

            // Rebuild indices after the removed position
            for (var i = idx; i < _entries.Count; i++)
            {
                _index[_entries[i].Key] = i;
            }
            return true;
        }

        /// <summary>
        /// Merges another context into this one. Existing keys are overwritten.
        /// </summary>
        public void Merge(FeelContext other)
        {
            if (other == null) return;
            foreach (var entry in other._entries)
            {
                Put(entry.Key, entry.Value);
            }
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _entries.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString()
        {
            var entries = string.Join(", ", _entries.Select(e => $"{e.Key}: {FormatValue(e.Value)}"));
            return $"{{{entries}}}";
        }

        private static string FormatValue(object value)
        {
            if (value == null) return "null";
            if (value is string s) return $"\"{s}\"";
            return value.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj is not FeelContext other) return false;
            if (Count != other.Count) return false;
            for (var i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].Key != other._entries[i].Key) return false;
                if (!Equals(_entries[i].Value, other._entries[i].Value)) return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();
            foreach (var entry in _entries)
            {
                hash.Add(entry.Key);
                hash.Add(entry.Value);
            }
            return hash.ToHashCode();
        }
    }
}

