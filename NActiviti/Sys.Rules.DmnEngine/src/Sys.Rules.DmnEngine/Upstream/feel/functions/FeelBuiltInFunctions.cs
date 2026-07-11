using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Sys.Rules.DmnEngine.Feel.Types;

namespace Sys.Rules.DmnEngine.Feel.Functions
{
    /// <summary>
    /// Registry and implementation of all FEEL built-in functions (~80 functions).
    /// Functions support both positional and named argument invocation.
    /// </summary>
    public static class FeelBuiltInFunctions
    {
        private static readonly Dictionary<string, FeelFunction> Functions = new();

        static FeelBuiltInFunctions()
        {
            RegisterStringFunctions();
            RegisterNumericFunctions();
            RegisterListFunctions();
            RegisterBooleanFunctions();
            RegisterDateTimeFunctions();
            RegisterConversionFunctions();
            RegisterContextFunctions();
            RegisterRangeFunctions();
        }

        /// <summary>
        /// Resolves a built-in function call by name.
        /// Returns null if the function is not found.
        /// </summary>
        public static object Resolve(string name, object[] args)
        {
            if (Functions.TryGetValue(name, out var fn))
                return fn.Invoke(args);
            return null;
        }

        /// <summary>
        /// Checks if a function with the given name exists.
        /// </summary>
        public static bool HasFunction(string name) => Functions.ContainsKey(name);

        /// <summary>
        /// Gets a built-in function by name, or null if not found.
        /// </summary>
        public static FeelFunction GetFunction(string name) =>
            Functions.TryGetValue(name, out var fn) ? fn : null;

        private static void Register(string name, string[] paramNames, Func<object[], object> body)
        {
            Functions[name] = new FeelFunction(name, paramNames, body);
        }

        private static object Arg(object[] args, int index) =>
            index < args.Length ? args[index] : null;

        private static decimal? ArgDecimal(object[] args, int index)
        {
            var val = Arg(args, index);
            if (val == null) return null;
            if (FeelValueComparer.IsNumeric(val)) return FeelValueComparer.ToDecimal(val);
            return null;
        }

        private static string ArgString(object[] args, int index) =>
            Arg(args, index) as string;

        private static List<object> ArgList(object[] args, int index)
        {
            var val = Arg(args, index);
            if (val is List<object> list) return list;
            if (val != null) return new List<object> { val };
            return null;
        }

        // ==================== String Functions ====================

        private static void RegisterStringFunctions()
        {
            Register("substring", new[] { "string", "start position", "length" }, args =>
            {
                var s = ArgString(args, 0);
                var start = ArgDecimal(args, 1);
                if (s == null || start == null) return null;
                var startIdx = (int)start.Value;
                // FEEL is 1-based
                var clrStart = startIdx > 0 ? startIdx - 1 : s.Length + startIdx;
                if (clrStart < 0) clrStart = 0;
                if (clrStart >= s.Length) return "";
                var length = ArgDecimal(args, 2);
                if (length != null)
                {
                    var len = (int)length.Value;
                    if (len < 0) return null;
                    return s.Substring(clrStart, Math.Min(len, s.Length - clrStart));
                }
                return s.Substring(clrStart);
            });

            Register("string length", new[] { "string" }, args =>
            {
                var s = ArgString(args, 0);
                return s != null ? (decimal)s.Length : null;
            });

            Register("upper case", new[] { "string" }, args =>
            {
                var s = ArgString(args, 0);
                return s?.ToUpperInvariant();
            });

            Register("lower case", new[] { "string" }, args =>
            {
                var s = ArgString(args, 0);
                return s?.ToLowerInvariant();
            });

            Register("substring before", new[] { "string", "match" }, args =>
            {
                var s = ArgString(args, 0);
                var match = ArgString(args, 1);
                if (s == null || match == null) return null;
                var idx = s.IndexOf(match, StringComparison.Ordinal);
                return idx < 0 ? "" : s.Substring(0, idx);
            });

            Register("substring after", new[] { "string", "match" }, args =>
            {
                var s = ArgString(args, 0);
                var match = ArgString(args, 1);
                if (s == null || match == null) return null;
                var idx = s.IndexOf(match, StringComparison.Ordinal);
                return idx < 0 ? "" : s.Substring(idx + match.Length);
            });

            Register("contains", new[] { "string", "match" }, args =>
            {
                var s = ArgString(args, 0);
                var match = ArgString(args, 1);
                if (s == null || match == null) return null;
                return (object)s.Contains(match, StringComparison.Ordinal);
            });

            Register("starts with", new[] { "string", "match" }, args =>
            {
                var s = ArgString(args, 0);
                var match = ArgString(args, 1);
                if (s == null || match == null) return null;
                return (object)s.StartsWith(match, StringComparison.Ordinal);
            });

            Register("ends with", new[] { "string", "match" }, args =>
            {
                var s = ArgString(args, 0);
                var match = ArgString(args, 1);
                if (s == null || match == null) return null;
                return (object)s.EndsWith(match, StringComparison.Ordinal);
            });

            Register("matches", new[] { "input", "pattern", "flags" }, args =>
            {
                var input = ArgString(args, 0);
                var pattern = ArgString(args, 1);
                if (input == null || pattern == null) return null;
                var flags = ArgString(args, 2);
                var regexOpts = RegexOptions.None;
                if (flags != null)
                {
                    if (flags.Contains('i')) regexOpts |= RegexOptions.IgnoreCase;
                    if (flags.Contains('m')) regexOpts |= RegexOptions.Multiline;
                    if (flags.Contains('s')) regexOpts |= RegexOptions.Singleline;
                }
                try
                {
                    return (object)Regex.IsMatch(input, pattern, regexOpts);
                }
                catch
                {
                    return null;
                }
            });

            Register("replace", new[] { "input", "pattern", "replacement", "flags" }, args =>
            {
                var input = ArgString(args, 0);
                var pattern = ArgString(args, 1);
                var replacement = ArgString(args, 2);
                if (input == null || pattern == null || replacement == null) return null;
                var flags = ArgString(args, 3);
                var regexOpts = RegexOptions.None;
                if (flags != null)
                {
                    if (flags.Contains('i')) regexOpts |= RegexOptions.IgnoreCase;
                    if (flags.Contains('m')) regexOpts |= RegexOptions.Multiline;
                    if (flags.Contains('s')) regexOpts |= RegexOptions.Singleline;
                }
                try
                {
                    return Regex.Replace(input, pattern, replacement, regexOpts);
                }
                catch
                {
                    return null;
                }
            });

            Register("split", new[] { "string", "delimiter" }, args =>
            {
                var s = ArgString(args, 0);
                var delim = ArgString(args, 1);
                if (s == null || delim == null) return null;
                return (object)s.Split(delim).Select(x => (object)x).ToList();
            });

            Register("string join", new[] { "list", "delimiter" }, args =>
            {
                var list = ArgList(args, 0);
                if (list == null) return null;
                var strings = list.Where(x => x is string).Cast<string>();
                var delim = ArgString(args, 1);
                return delim != null ? string.Join(delim, strings) : string.Join("", strings);
            });
        }

        // ==================== Numeric Functions ====================

        private static void RegisterNumericFunctions()
        {
            Register("decimal", new[] { "n", "scale" }, args =>
            {
                var n = ArgDecimal(args, 0);
                var scale = ArgDecimal(args, 1);
                if (n == null || scale == null) return null;
                return Math.Round(n.Value, (int)scale.Value, MidpointRounding.AwayFromZero);
            });

            Register("floor", new[] { "n" }, args =>
            {
                var n = ArgDecimal(args, 0);
                if (n == null) return null;
                return args.Length > 1 && ArgDecimal(args, 1) is decimal scale
                    ? Math.Floor(n.Value * PowerOf10(scale)) / PowerOf10(scale)
                    : Math.Floor(n.Value);
            });

            Register("ceiling", new[] { "n" }, args =>
            {
                var n = ArgDecimal(args, 0);
                if (n == null) return null;
                return args.Length > 1 && ArgDecimal(args, 1) is decimal scale
                    ? Math.Ceiling(n.Value * PowerOf10(scale)) / PowerOf10(scale)
                    : Math.Ceiling(n.Value);
            });

            Register("round up", new[] { "n", "scale" }, args =>
            {
                var n = ArgDecimal(args, 0);
                var scale = ArgDecimal(args, 1);
                if (n == null || scale == null) return null;
                var factor = PowerOf10(scale.Value);
                return n.Value >= 0
                    ? Math.Ceiling(n.Value * factor) / factor
                    : Math.Floor(n.Value * factor) / factor;
            });

            Register("round down", new[] { "n", "scale" }, args =>
            {
                var n = ArgDecimal(args, 0);
                var scale = ArgDecimal(args, 1);
                if (n == null || scale == null) return null;
                var factor = PowerOf10(scale.Value);
                return n.Value >= 0
                    ? Math.Floor(n.Value * factor) / factor
                    : Math.Ceiling(n.Value * factor) / factor;
            });

            Register("round half up", new[] { "n", "scale" }, args =>
            {
                var n = ArgDecimal(args, 0);
                var scale = ArgDecimal(args, 1);
                if (n == null || scale == null) return null;
                return Math.Round(n.Value, (int)scale.Value, MidpointRounding.AwayFromZero);
            });

            Register("round half down", new[] { "n", "scale" }, args =>
            {
                var n = ArgDecimal(args, 0);
                var scale = ArgDecimal(args, 1);
                if (n == null || scale == null) return null;
                return Math.Round(n.Value, (int)scale.Value, MidpointRounding.ToEven);
            });

            Register("abs", new[] { "n" }, args =>
            {
                var val = Arg(args, 0);
                if (val == null) return null;
                if (FeelValueComparer.IsNumeric(val)) return Math.Abs(FeelValueComparer.ToDecimal(val));
                if (val is TimeSpan ts) return ts < TimeSpan.Zero ? -ts : ts;
                if (val is FeelYmDuration ym) return new FeelYmDuration(Math.Abs(ym.TotalMonths));
                return null;
            });

            Register("modulo", new[] { "dividend", "divisor" }, args =>
            {
                var a = ArgDecimal(args, 0);
                var b = ArgDecimal(args, 1);
                if (a == null || b == null || b == 0) return null;
                return a.Value % b.Value;
            });

            Register("sqrt", new[] { "number" }, args =>
            {
                var n = ArgDecimal(args, 0);
                if (n == null || n.Value < 0) return null;
                return (decimal)Math.Sqrt((double)n.Value);
            });

            Register("log", new[] { "number" }, args =>
            {
                var n = ArgDecimal(args, 0);
                if (n == null || n.Value <= 0) return null;
                return (decimal)Math.Log((double)n.Value);
            });

            Register("exp", new[] { "number" }, args =>
            {
                var n = ArgDecimal(args, 0);
                if (n == null) return null;
                return (decimal)Math.Exp((double)n.Value);
            });

            Register("odd", new[] { "number" }, args =>
            {
                var n = ArgDecimal(args, 0);
                if (n == null) return null;
                return (object)((int)n.Value % 2 != 0);
            });

            Register("even", new[] { "number" }, args =>
            {
                var n = ArgDecimal(args, 0);
                if (n == null) return null;
                return (object)((int)n.Value % 2 == 0);
            });
        }

        private static decimal PowerOf10(decimal scale)
        {
            var s = (int)scale;
            decimal result = 1;
            for (var i = 0; i < Math.Abs(s); i++) result *= 10;
            return s >= 0 ? result : 1 / result;
        }

        // ==================== List Functions ====================

        private static void RegisterListFunctions()
        {
            Register("list contains", new[] { "list", "element" }, args =>
            {
                var list = ArgList(args, 0);
                var element = Arg(args, 1);
                if (list == null) return null;
                return (object)list.Any(item => FeelValueComparer.FeelEqual(item, element) == true);
            });

            Register("count", new[] { "list" }, args =>
            {
                var list = ArgList(args, 0);
                if (list == null) return 0m;
                return (decimal)list.Count;
            });

            Register("min", new[] { "list" }, args =>
            {
                var list = FlattenArgs(args);
                if (list == null || list.Count == 0) return null;
                return list.Where(x => x != null).Aggregate((a, b) =>
                    FeelValueComparer.Compare(a, b) <= 0 ? a : b);
            });

            Register("max", new[] { "list" }, args =>
            {
                var list = FlattenArgs(args);
                if (list == null || list.Count == 0) return null;
                return list.Where(x => x != null).Aggregate((a, b) =>
                    FeelValueComparer.Compare(a, b) >= 0 ? a : b);
            });

            Register("sum", new[] { "list" }, args =>
            {
                var list = FlattenArgs(args);
                if (list == null || list.Count == 0) return null;
                decimal total = 0;
                foreach (var item in list)
                {
                    if (item == null || !FeelValueComparer.IsNumeric(item)) return null;
                    total += FeelValueComparer.ToDecimal(item);
                }
                return total;
            });

            Register("mean", new[] { "list" }, args =>
            {
                var list = FlattenArgs(args);
                if (list == null || list.Count == 0) return null;
                decimal total = 0;
                var count = 0;
                foreach (var item in list)
                {
                    if (item == null || !FeelValueComparer.IsNumeric(item)) return null;
                    total += FeelValueComparer.ToDecimal(item);
                    count++;
                }
                return count > 0 ? total / count : null;
            });

            Register("all", new[] { "list" }, args =>
            {
                var list = FlattenArgs(args);
                if (list == null || list.Count == 0) return true;
                bool? result = true;
                foreach (var item in list)
                {
                    var b = FeelValueComparer.ToBool(item);
                    result = FeelValueComparer.FeelAnd(result, b);
                    if (result == false) return false;
                }
                return result;
            });

            Register("any", new[] { "list" }, args =>
            {
                var list = FlattenArgs(args);
                if (list == null || list.Count == 0) return false;
                bool? result = false;
                foreach (var item in list)
                {
                    var b = FeelValueComparer.ToBool(item);
                    result = FeelValueComparer.FeelOr(result, b);
                    if (result == true) return true;
                }
                return result;
            });

            Register("sublist", new[] { "list", "start position", "length" }, args =>
            {
                var list = ArgList(args, 0);
                var start = ArgDecimal(args, 1);
                if (list == null || start == null) return null;
                var startIdx = (int)start.Value - 1; // 1-based
                if (startIdx < 0) startIdx = list.Count + startIdx + 1;
                var length = ArgDecimal(args, 2);
                var len = length != null ? (int)length.Value : list.Count - startIdx;
                if (startIdx < 0 || startIdx >= list.Count) return new List<object>();
                len = Math.Min(len, list.Count - startIdx);
                return list.GetRange(startIdx, len).ToList();
            });

            Register("append", new[] { "list", "item" }, args =>
            {
                var list = ArgList(args, 0);
                if (list == null) return null;
                var result = new List<object>(list);
                for (var i = 1; i < args.Length; i++)
                    result.Add(Arg(args, i));
                return (object)result;
            });

            Register("concatenate", new[] { "list" }, args =>
            {
                var result = new List<object>();
                foreach (var arg in args)
                {
                    if (arg is List<object> list)
                        result.AddRange(list);
                    else if (arg != null)
                        result.Add(arg);
                }
                return (object)result;
            });

            Register("insert before", new[] { "list", "position", "newItem" }, args =>
            {
                var list = ArgList(args, 0);
                var pos = ArgDecimal(args, 1);
                var newItem = Arg(args, 2);
                if (list == null || pos == null) return null;
                var result = new List<object>(list);
                var idx = (int)pos.Value - 1; // 1-based
                if (idx < 0) idx = result.Count + idx + 1;
                if (idx < 0) idx = 0;
                if (idx > result.Count) idx = result.Count;
                result.Insert(idx, newItem);
                return (object)result;
            });

            Register("remove", new[] { "list", "position" }, args =>
            {
                var list = ArgList(args, 0);
                var pos = ArgDecimal(args, 1);
                if (list == null || pos == null) return null;
                var result = new List<object>(list);
                var idx = (int)pos.Value - 1; // 1-based
                if (idx < 0 || idx >= result.Count) return (object)result;
                result.RemoveAt(idx);
                return (object)result;
            });

            Register("reverse", new[] { "list" }, args =>
            {
                var list = ArgList(args, 0);
                if (list == null) return null;
                var result = new List<object>(list);
                result.Reverse();
                return (object)result;
            });

            Register("index of", new[] { "list", "match" }, args =>
            {
                var list = ArgList(args, 0);
                var match = Arg(args, 1);
                if (list == null) return null;
                var result = new List<object>();
                for (var i = 0; i < list.Count; i++)
                {
                    if (FeelValueComparer.FeelEqual(list[i], match) == true)
                        result.Add((decimal)(i + 1)); // 1-based
                }
                return (object)result;
            });

            Register("union", new[] { "list" }, args =>
            {
                var result = new List<object>();
                foreach (var arg in args)
                {
                    if (arg is List<object> list)
                    {
                        foreach (var item in list)
                        {
                            if (!result.Any(existing => FeelValueComparer.FeelEqual(existing, item) == true))
                                result.Add(item);
                        }
                    }
                }
                return (object)result;
            });

            Register("distinct values", new[] { "list" }, args =>
            {
                var list = ArgList(args, 0);
                if (list == null) return null;
                var result = new List<object>();
                foreach (var item in list)
                {
                    if (!result.Any(existing => FeelValueComparer.FeelEqual(existing, item) == true))
                        result.Add(item);
                }
                return (object)result;
            });

            Register("flatten", new[] { "list" }, args =>
            {
                var list = ArgList(args, 0);
                if (list == null) return null;
                var result = new List<object>();
                FlattenRecursive(list, result);
                return (object)result;
            });

            Register("product", new[] { "list" }, args =>
            {
                var list = FlattenArgs(args);
                if (list == null || list.Count == 0) return null;
                decimal product = 1;
                foreach (var item in list)
                {
                    if (item == null || !FeelValueComparer.IsNumeric(item)) return null;
                    product *= FeelValueComparer.ToDecimal(item);
                }
                return product;
            });

            Register("median", new[] { "list" }, args =>
            {
                var list = FlattenArgs(args);
                if (list == null || list.Count == 0) return null;
                var nums = new List<decimal>();
                foreach (var item in list)
                {
                    if (item == null || !FeelValueComparer.IsNumeric(item)) return null;
                    nums.Add(FeelValueComparer.ToDecimal(item));
                }
                nums.Sort();
                var n = nums.Count;
                return n % 2 == 1 ? nums[n / 2] : (nums[n / 2 - 1] + nums[n / 2]) / 2;
            });

            Register("stddev", new[] { "list" }, args =>
            {
                var list = FlattenArgs(args);
                if (list == null || list.Count < 2) return null;
                var nums = new List<decimal>();
                foreach (var item in list)
                {
                    if (item == null || !FeelValueComparer.IsNumeric(item)) return null;
                    nums.Add(FeelValueComparer.ToDecimal(item));
                }
                var mean = nums.Sum() / nums.Count;
                var variance = nums.Sum(x => (x - mean) * (x - mean)) / (nums.Count - 1);
                return (decimal)Math.Sqrt((double)variance);
            });

            Register("mode", new[] { "list" }, args =>
            {
                var list = FlattenArgs(args);
                if (list == null || list.Count == 0) return (object)new List<object>();
                var groups = new List<(object value, int count)>();
                foreach (var item in list)
                {
                    var found = false;
                    for (var i = 0; i < groups.Count; i++)
                    {
                        if (FeelValueComparer.FeelEqual(groups[i].value, item) == true)
                        {
                            groups[i] = (groups[i].value, groups[i].count + 1);
                            found = true;
                            break;
                        }
                    }
                    if (!found) groups.Add((item, 1));
                }
                var maxCount = groups.Max(g => g.count);
                return (object)groups.Where(g => g.count == maxCount).Select(g => g.value).ToList();
            });

            Register("sort", new[] { "list", "precedes" }, args =>
            {
                var list = ArgList(args, 0);
                if (list == null) return null;
                var precedes = Arg(args, 1) as FeelFunction;
                var result = new List<object>(list);

                if (precedes != null)
                {
                    result.Sort((a, b) =>
                    {
                        var r = precedes.Invoke(a, b);
                        if (r is bool bv && bv) return -1;
                        return 1;
                    });
                }
                else
                {
                    result.Sort((a, b) => FeelValueComparer.Compare(a, b) ?? 0);
                }
                return (object)result;
            });
        }

        private static void FlattenRecursive(List<object> source, List<object> result)
        {
            foreach (var item in source)
            {
                if (item is List<object> nested)
                    FlattenRecursive(nested, result);
                else
                    result.Add(item);
            }
        }

        /// <summary>
        /// If a single list argument is passed, use it. Otherwise treat all args as the list.
        /// This supports both sum([1,2,3]) and sum(1,2,3) calling conventions.
        /// </summary>
        private static List<object> FlattenArgs(object[] args)
        {
            if (args.Length == 1 && args[0] is List<object> list) return list;
            return args.ToList();
        }

        // ==================== Boolean Functions ====================

        private static void RegisterBooleanFunctions()
        {
            Register("not", new[] { "negand" }, args =>
            {
                var b = FeelValueComparer.ToBool(Arg(args, 0));
                return FeelValueComparer.FeelNot(b);
            });

            Register("is", new[] { "value1", "value2" }, args =>
            {
                var a = Arg(args, 0);
                var b = Arg(args, 1);
                return FeelValueComparer.FeelEqual(a, b);
            });
        }

        // ==================== Date/Time Functions ====================

        private static void RegisterDateTimeFunctions()
        {
            Register("date", new[] { "from", "year", "month" }, args =>
            {
                if (args.Length >= 3)
                {
                    var y = ArgDecimal(args, 0);
                    var m = ArgDecimal(args, 1);
                    var d = ArgDecimal(args, 2);
                    if (y == null || m == null || d == null) return null;
                    return new DateOnly((int)y.Value, (int)m.Value, (int)d.Value);
                }
                var val = Arg(args, 0);
                if (val is string s)
                {
                    if (DateOnly.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                        return d;
                    return null;
                }
                if (val is DateTimeOffset dto) return DateOnly.FromDateTime(dto.DateTime);
                if (val is DateOnly) return val;
                return null;
            });

            Register("time", new[] { "from", "hour", "minute", "second" }, args =>
            {
                if (args.Length >= 3)
                {
                    var h = ArgDecimal(args, 0);
                    var m = ArgDecimal(args, 1);
                    var s = ArgDecimal(args, 2);
                    if (h == null || m == null || s == null) return null;
                    TimeSpan? offset = null;
                    if (args.Length >= 4 && Arg(args, 3) is TimeSpan tz) offset = tz;
                    return new FeelTime((int)h.Value, (int)m.Value, (int)s.Value, offset);
                }
                var val = Arg(args, 0);
                if (val is string str)
                {
                    if (TryParseTime(str, out var t)) return t;
                    return null;
                }
                if (val is FeelTime) return val;
                if (val is DateTimeOffset dto)
                    return new FeelTime(TimeOnly.FromTimeSpan(dto.TimeOfDay), dto.Offset);
                return null;
            });

            Register("date and time", new[] { "from", "time" }, args =>
            {
                if (args.Length >= 2)
                {
                    var dateVal = Arg(args, 0);
                    var timeVal = Arg(args, 1);

                    DateOnly date;
                    if (dateVal is DateOnly d) date = d;
                    else if (dateVal is DateTimeOffset dto2) date = DateOnly.FromDateTime(dto2.DateTime);
                    else if (dateVal is string ds && DateOnly.TryParseExact(ds, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dp))
                        date = dp;
                    else return null;

                    if (timeVal is FeelTime ft)
                        return new DateTimeOffset(date.ToDateTime(ft.Time), ft.Offset ?? TimeSpan.Zero);
                    if (timeVal is string ts && TryParseTime(ts, out var t))
                        return new DateTimeOffset(date.ToDateTime(t.Time), t.Offset ?? TimeSpan.Zero);
                    return null;
                }
                var val = Arg(args, 0);
                if (val is string s)
                {
                    if (DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto))
                        return dto;
                    return null;
                }
                if (val is DateTimeOffset) return val;
                return null;
            });

            Register("duration", new[] { "from" }, args =>
            {
                var s = ArgString(args, 0);
                if (s == null) return null;
                return ParseDuration(s);
            });

            Register("years and months duration", new[] { "from", "to" }, args =>
            {
                var from = Arg(args, 0);
                var to = Arg(args, 1);
                DateOnly fromDate, toDate;
                if (from is DateOnly fd) fromDate = fd;
                else if (from is DateTimeOffset fdt) fromDate = DateOnly.FromDateTime(fdt.DateTime);
                else return null;
                if (to is DateOnly td) toDate = td;
                else if (to is DateTimeOffset tdt) toDate = DateOnly.FromDateTime(tdt.DateTime);
                else return null;

                var months = (toDate.Year - fromDate.Year) * 12 + (toDate.Month - fromDate.Month);
                return new FeelYmDuration(months);
            });

            Register("now", Array.Empty<string>(), _ => DateTimeOffset.Now);
            Register("today", Array.Empty<string>(), _ => DateOnly.FromDateTime(DateTime.Today));

            Register("day of year", new[] { "date" }, args =>
            {
                var val = Arg(args, 0);
                if (val is DateOnly d) return (decimal)d.DayOfYear;
                if (val is DateTimeOffset dto) return (decimal)dto.DayOfYear;
                return null;
            });

            Register("day of week", new[] { "date" }, args =>
            {
                var val = Arg(args, 0);
                DayOfWeek dow;
                if (val is DateOnly d) dow = d.DayOfWeek;
                else if (val is DateTimeOffset dto) dow = dto.DayOfWeek;
                else return null;
                // FEEL: Monday=1 .. Sunday=7
                return dow == DayOfWeek.Sunday ? 7m : (decimal)(int)dow;
            });

            Register("month of year", new[] { "date" }, args =>
            {
                var val = Arg(args, 0);
                if (val is DateOnly d) return (decimal)d.Month;
                if (val is DateTimeOffset dto) return (decimal)dto.Month;
                return null;
            });

            Register("week of year", new[] { "date" }, args =>
            {
                var val = Arg(args, 0);
                DateTime dt;
                if (val is DateOnly d) dt = d.ToDateTime(TimeOnly.MinValue);
                else if (val is DateTimeOffset dto) dt = dto.DateTime;
                else return null;
                return (decimal)ISOWeek.GetWeekOfYear(dt);
            });
        }

        private static bool TryParseTime(string raw, out FeelTime result)
        {
            result = default;
            var match = Regex.Match(raw, @"^(\d{1,2}):(\d{2})(?::(\d{2})(?:\.(\d+))?)?(?:(Z)|([+-]\d{2}:\d{2}))?$");
            if (!match.Success) return false;
            var hour = int.Parse(match.Groups[1].Value);
            var minute = int.Parse(match.Groups[2].Value);
            var second = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 0;
            TimeSpan? offset = null;
            if (match.Groups[5].Success) offset = TimeSpan.Zero;
            else if (match.Groups[6].Success) offset = TimeSpan.Parse(match.Groups[6].Value);
            result = new FeelTime(hour, minute, second, offset);
            return true;
        }

        private static object ParseDuration(string s)
        {
            var negative = s.StartsWith("-");
            var raw = negative ? s.Substring(1) : s;
            if (!raw.StartsWith("P")) return null;
            raw = raw.Substring(1);

            if (!raw.Contains('T') && !raw.Contains('D') && !raw.Contains('H') && !raw.Contains('S'))
            {
                var years = 0;
                var months = 0;
                var yMatch = Regex.Match(raw, @"(\d+)Y");
                if (yMatch.Success) years = int.Parse(yMatch.Groups[1].Value);
                var mMatch = Regex.Match(raw, @"(\d+)M");
                if (mMatch.Success) months = int.Parse(mMatch.Groups[1].Value);
                var total = years * 12 + months;
                if (negative) total = -total;
                return new FeelYmDuration(total);
            }

            var days = 0;
            var hours = 0;
            var minutes = 0;
            var seconds = 0;
            var dMatch = Regex.Match(raw, @"(\d+)D");
            if (dMatch.Success) days = int.Parse(dMatch.Groups[1].Value);
            var tIdx = raw.IndexOf('T');
            if (tIdx >= 0)
            {
                var timePart = raw.Substring(tIdx + 1);
                var hMatch = Regex.Match(timePart, @"(\d+)H");
                if (hMatch.Success) hours = int.Parse(hMatch.Groups[1].Value);
                var minMatch = Regex.Match(timePart, @"(\d+)M");
                if (minMatch.Success) minutes = int.Parse(minMatch.Groups[1].Value);
                var sMatch = Regex.Match(timePart, @"(\d+)S");
                if (sMatch.Success) seconds = int.Parse(sMatch.Groups[1].Value);
            }
            var ts = new TimeSpan(days, hours, minutes, seconds);
            if (negative) ts = -ts;
            return ts;
        }

        // ==================== Conversion Functions ====================

        private static void RegisterConversionFunctions()
        {
            Register("number", new[] { "from", "grouping separator", "decimal separator" }, args =>
            {
                var val = Arg(args, 0);
                if (val == null) return null;
                if (FeelValueComparer.IsNumeric(val)) return FeelValueComparer.ToDecimal(val);
                if (val is string s)
                {
                    var groupSep = ArgString(args, 1);
                    var decSep = ArgString(args, 2);
                    if (groupSep != null) s = s.Replace(groupSep, "");
                    if (decSep != null && decSep != ".") s = s.Replace(decSep, ".");
                    if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                        return d;
                }
                return null;
            });

            Register("string", new[] { "from" }, args =>
            {
                var val = Arg(args, 0);
                if (val == null) return null;
                if (val is string s) return s;
                if (val is bool b) return b ? "true" : "false";
                if (FeelValueComparer.IsNumeric(val))
                    return FeelValueComparer.ToDecimal(val).ToString(CultureInfo.InvariantCulture);
                if (val is DateOnly d) return d.ToString("yyyy-MM-dd");
                if (val is DateTimeOffset dto) return dto.ToString("yyyy-MM-ddTHH:mm:sszzz");
                if (val is FeelTime ft) return ft.ToString();
                if (val is TimeSpan ts) return FormatTimeSpanDuration(ts);
                if (val is FeelYmDuration ym) return ym.ToString();
                return val.ToString();
            });
        }

        private static string FormatTimeSpanDuration(TimeSpan ts)
        {
            var neg = ts < TimeSpan.Zero;
            if (neg) ts = -ts;
            var parts = new List<string>();
            if (ts.Days > 0) parts.Add($"{ts.Days}D");
            var timeParts = new List<string>();
            if (ts.Hours > 0) timeParts.Add($"{ts.Hours}H");
            if (ts.Minutes > 0) timeParts.Add($"{ts.Minutes}M");
            if (ts.Seconds > 0) timeParts.Add($"{ts.Seconds}S");
            var result = "P" + string.Join("", parts);
            if (timeParts.Count > 0) result += "T" + string.Join("", timeParts);
            else if (parts.Count == 0) result += "T0S";
            return neg ? $"-{result}" : result;
        }

        // ==================== Context Functions ====================

        private static void RegisterContextFunctions()
        {
            Register("get value", new[] { "m", "key" }, args =>
            {
                var ctx = Arg(args, 0) as FeelContext;
                var key = ArgString(args, 1);
                if (ctx == null || key == null) return null;
                return ctx[key];
            });

            Register("get entries", new[] { "m" }, args =>
            {
                var ctx = Arg(args, 0) as FeelContext;
                if (ctx == null) return null;
                var result = new List<object>();
                foreach (var entry in ctx)
                {
                    var entryCtx = new FeelContext();
                    entryCtx.Put("key", entry.Key);
                    entryCtx.Put("value", entry.Value);
                    result.Add(entryCtx);
                }
                return (object)result;
            });

            Register("context", new[] { "entries" }, args =>
            {
                var entries = ArgList(args, 0);
                if (entries == null) return null;
                var result = new FeelContext();
                foreach (var entry in entries)
                {
                    if (entry is FeelContext entryCtx)
                    {
                        var key = entryCtx["key"] as string;
                        if (key != null) result.Put(key, entryCtx["value"]);
                    }
                }
                return result;
            });

            Register("context put", new[] { "context", "key", "value" }, args =>
            {
                var ctx = Arg(args, 0) as FeelContext;
                if (ctx == null) return null;
                var result = new FeelContext();
                result.Merge(ctx);

                if (args.Length >= 3)
                {
                    var key = ArgString(args, 1);
                    if (key == null) return null;
                    result.Put(key, Arg(args, 2));
                }
                else if (Arg(args, 1) is FeelContext additions)
                {
                    result.Merge(additions);
                }

                return result;
            });

            Register("context merge", new[] { "contexts" }, args =>
            {
                var list = ArgList(args, 0);
                if (list == null) return null;
                var result = new FeelContext();
                foreach (var item in list)
                {
                    if (item is FeelContext ctx) result.Merge(ctx);
                }
                return result;
            });
        }

        // ==================== Range Functions ====================

        private static void RegisterRangeFunctions()
        {
            Register("before", new[] { "point1", "point2" }, args =>
            {
                var a = Arg(args, 0);
                var b = Arg(args, 1);
                return RangeRelation(a, b, "before");
            });

            Register("after", new[] { "point1", "point2" }, args =>
            {
                var a = Arg(args, 0);
                var b = Arg(args, 1);
                return RangeRelation(a, b, "after");
            });

            Register("meets", new[] { "range1", "range2" }, args =>
            {
                var a = Arg(args, 0) as FeelRange;
                var b = Arg(args, 1) as FeelRange;
                if (a == null || b == null) return null;
                return FeelValueComparer.FeelEqual(a.HighEndpoint, b.LowEndpoint) == true
                    && a.HighInclusive && b.LowInclusive;
            });

            Register("met by", new[] { "range1", "range2" }, args =>
            {
                var a = Arg(args, 0) as FeelRange;
                var b = Arg(args, 1) as FeelRange;
                if (a == null || b == null) return null;
                return FeelValueComparer.FeelEqual(a.LowEndpoint, b.HighEndpoint) == true
                    && a.LowInclusive && b.HighInclusive;
            });

            Register("overlaps", new[] { "range1", "range2" }, args =>
            {
                var a = Arg(args, 0) as FeelRange;
                var b = Arg(args, 1) as FeelRange;
                if (a == null || b == null) return null;
                // Two ranges overlap if neither is completely before the other
                var aBeforeB = FeelValueComparer.Compare(a.HighEndpoint, b.LowEndpoint);
                var bBeforeA = FeelValueComparer.Compare(b.HighEndpoint, a.LowEndpoint);
                if (aBeforeB == null || bBeforeA == null) return null;
                var aEndsBefore = aBeforeB < 0 || (aBeforeB == 0 && !(a.HighInclusive && b.LowInclusive));
                var bEndsBefore = bBeforeA < 0 || (bBeforeA == 0 && !(b.HighInclusive && a.LowInclusive));
                return (object)(!aEndsBefore && !bEndsBefore);
            });

            Register("includes", new[] { "range", "point" }, args =>
            {
                var range = Arg(args, 0) as FeelRange;
                var point = Arg(args, 1);
                if (range == null) return null;
                if (point is FeelRange inner)
                {
                    // Range includes range
                    var lowOk = FeelValueComparer.Compare(range.LowEndpoint, inner.LowEndpoint);
                    var highOk = FeelValueComparer.Compare(range.HighEndpoint, inner.HighEndpoint);
                    if (lowOk == null || highOk == null) return null;
                    return (object)(lowOk <= 0 && highOk >= 0);
                }
                return range.Contains(point);
            });

            Register("during", new[] { "point", "range" }, args =>
            {
                var point = Arg(args, 0);
                var range = Arg(args, 1) as FeelRange;
                if (range == null) return null;
                if (point is FeelRange inner)
                {
                    var lowOk = FeelValueComparer.Compare(range.LowEndpoint, inner.LowEndpoint);
                    var highOk = FeelValueComparer.Compare(range.HighEndpoint, inner.HighEndpoint);
                    if (lowOk == null || highOk == null) return null;
                    return (object)(lowOk <= 0 && highOk >= 0);
                }
                return range.Contains(point);
            });

            Register("starts", new[] { "point", "range" }, args =>
            {
                var point = Arg(args, 0);
                var range = Arg(args, 1) as FeelRange;
                if (range == null) return null;
                return FeelValueComparer.FeelEqual(point, range.LowEndpoint);
            });

            Register("started by", new[] { "range", "point" }, args =>
            {
                var range = Arg(args, 0) as FeelRange;
                var point = Arg(args, 1);
                if (range == null) return null;
                return FeelValueComparer.FeelEqual(range.LowEndpoint, point);
            });

            Register("finishes", new[] { "point", "range" }, args =>
            {
                var point = Arg(args, 0);
                var range = Arg(args, 1) as FeelRange;
                if (range == null) return null;
                return FeelValueComparer.FeelEqual(point, range.HighEndpoint);
            });

            Register("finished by", new[] { "range", "point" }, args =>
            {
                var range = Arg(args, 0) as FeelRange;
                var point = Arg(args, 1);
                if (range == null) return null;
                return FeelValueComparer.FeelEqual(range.HighEndpoint, point);
            });

            Register("coincides", new[] { "point1", "point2" }, args =>
            {
                var a = Arg(args, 0);
                var b = Arg(args, 1);
                if (a is FeelRange ra && b is FeelRange rb)
                {
                    return (object)(FeelValueComparer.FeelEqual(ra.LowEndpoint, rb.LowEndpoint) == true
                        && FeelValueComparer.FeelEqual(ra.HighEndpoint, rb.HighEndpoint) == true
                        && ra.LowInclusive == rb.LowInclusive
                        && ra.HighInclusive == rb.HighInclusive);
                }
                return FeelValueComparer.FeelEqual(a, b);
            });
        }

        private static object RangeRelation(object a, object b, string relation)
        {
            // Point before/after point
            if (a is not FeelRange && b is not FeelRange)
            {
                var cmp = FeelValueComparer.Compare(a, b);
                if (cmp == null) return null;
                return relation == "before" ? (object)(cmp < 0) : (object)(cmp > 0);
            }

            // Point before/after range or range before/after point
            if (a is FeelRange ra && b is not FeelRange)
            {
                var cmp = FeelValueComparer.Compare(ra.HighEndpoint, b);
                if (cmp == null) return null;
                if (relation == "before")
                    return (object)(cmp < 0 || (cmp == 0 && !ra.HighInclusive));
                cmp = FeelValueComparer.Compare(ra.LowEndpoint, b);
                if (cmp == null) return null;
                return (object)(cmp > 0 || (cmp == 0 && !ra.LowInclusive));
            }

            if (a is not FeelRange && b is FeelRange rb)
            {
                var cmp = FeelValueComparer.Compare(a, rb.LowEndpoint);
                if (cmp == null) return null;
                if (relation == "before")
                    return (object)(cmp < 0 || (cmp == 0 && !rb.LowInclusive));
                cmp = FeelValueComparer.Compare(a, rb.HighEndpoint);
                if (cmp == null) return null;
                return (object)(cmp > 0 || (cmp == 0 && !rb.HighInclusive));
            }

            // Range before/after range
            if (a is FeelRange ra2 && b is FeelRange rb2)
            {
                if (relation == "before")
                {
                    var cmp = FeelValueComparer.Compare(ra2.HighEndpoint, rb2.LowEndpoint);
                    if (cmp == null) return null;
                    return (object)(cmp < 0 || (cmp == 0 && !(ra2.HighInclusive && rb2.LowInclusive)));
                }
                else
                {
                    var cmp = FeelValueComparer.Compare(ra2.LowEndpoint, rb2.HighEndpoint);
                    if (cmp == null) return null;
                    return (object)(cmp > 0 || (cmp == 0 && !(ra2.LowInclusive && rb2.HighInclusive)));
                }
            }

            return null;
        }
    }
}

