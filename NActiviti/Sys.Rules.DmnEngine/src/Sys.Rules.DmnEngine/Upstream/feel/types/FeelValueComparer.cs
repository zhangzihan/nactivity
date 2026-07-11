using System;
using System.Collections.Generic;

namespace Sys.Rules.DmnEngine.Feel.Types
{
    /// <summary>
    /// FEEL equality and ordering semantics.
    /// Handles null propagation, three-valued logic, and cross-type numeric comparison.
    /// </summary>
    public static class FeelValueComparer
    {
        /// <summary>
        /// FEEL equality: null = null is true; null = x is false (not null).
        /// Compares values using FEEL semantics.
        /// </summary>
        public static bool? FeelEqual(object left, object right)
        {
            // null = null 鈫?true
            if (left == null && right == null) return true;
            // null = x or x = null 鈫?false (per FEEL spec, not null)
            if (left == null || right == null) return false;

            // Numeric comparison with coercion
            if (IsNumeric(left) && IsNumeric(right))
                return ToDecimal(left) == ToDecimal(right);

            // Cross-type date/time comparison
            if ((left is DateOnly || left is DateTimeOffset) && (right is DateOnly || right is DateTimeOffset))
            {
                var cmp = Compare(left, right);
                return cmp == 0;
            }
            if ((left is FeelTime || left is DateTimeOffset) && (right is FeelTime || right is DateTimeOffset)
                && (left is FeelTime || right is FeelTime))
            {
                var cmp = Compare(left, right);
                return cmp == 0;
            }

            // Same type comparison
            if (left.GetType() == right.GetType())
                return left.Equals(right);

            // Cross-type: try Equals
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two FEEL values for ordering.
        /// Returns negative if left &lt; right, zero if equal, positive if left &gt; right.
        /// Returns null if values are not comparable.
        /// </summary>
        public static int? Compare(object left, object right)
        {
            if (left == null || right == null) return null;

            // Numeric comparison
            if (IsNumeric(left) && IsNumeric(right))
                return ToDecimal(left).CompareTo(ToDecimal(right));

            // String comparison
            if (left is string ls && right is string rs)
                return string.Compare(ls, rs, StringComparison.Ordinal);

            // Boolean comparison
            if (left is bool lb && right is bool rb)
                return lb.CompareTo(rb);

            // Date comparison
            if (left is DateOnly ld && right is DateOnly rd)
                return ld.CompareTo(rd);

            // DateTime comparison
            if (left is DateTimeOffset ldt && right is DateTimeOffset rdt)
                return ldt.CompareTo(rdt);

            // Cross-type: DateOnly vs DateTimeOffset
            if (left is DateOnly ld2 && right is DateTimeOffset rdt2)
                return new DateTimeOffset(ld2.ToDateTime(TimeOnly.MinValue)).CompareTo(rdt2);
            if (left is DateTimeOffset ldt2 && right is DateOnly rd2)
                return ldt2.CompareTo(new DateTimeOffset(rd2.ToDateTime(TimeOnly.MinValue)));

            // TimeSpan (days-and-time duration)
            if (left is TimeSpan lts && right is TimeSpan rts)
                return lts.CompareTo(rts);

            // FEEL types
            if (left is FeelTime lt && right is FeelTime rt)
                return lt.CompareTo(rt);

            // Cross-type: DateTimeOffset vs FeelTime (compare time portions)
            if (left is DateTimeOffset ldtf && right is FeelTime rft)
                return new FeelTime(ldtf.Hour, ldtf.Minute, ldtf.Second, null).CompareTo(rft);
            if (left is FeelTime lft && right is DateTimeOffset rdtf)
                return lft.CompareTo(new FeelTime(rdtf.Hour, rdtf.Minute, rdtf.Second, null));

            if (left is FeelYmDuration ly && right is FeelYmDuration ry)
                return ly.CompareTo(ry);

            // IComparable fallback for same types
            if (left.GetType() == right.GetType() && left is IComparable lc)
                return lc.CompareTo(right);

            return null; // not comparable
        }

        /// <summary>
        /// Three-valued FEEL 'and' operation
        /// </summary>
        public static bool? FeelAnd(bool? left, bool? right)
        {
            if (left == false || right == false) return false;
            if (left == true && right == true) return true;
            return null;
        }

        /// <summary>
        /// Three-valued FEEL 'or' operation
        /// </summary>
        public static bool? FeelOr(bool? left, bool? right)
        {
            if (left == true || right == true) return true;
            if (left == false && right == false) return false;
            return null;
        }

        /// <summary>
        /// Three-valued FEEL 'not' operation
        /// </summary>
        public static bool? FeelNot(bool? value)
        {
            if (value == null) return null;
            return !value.Value;
        }

        /// <summary>
        /// Checks if the value is a numeric type
        /// </summary>
        public static bool IsNumeric(object value)
        {
            return value is decimal or int or long or double or float or short or byte
                or uint or ulong or ushort or sbyte;
        }

        /// <summary>
        /// Converts a numeric value to decimal for FEEL arithmetic
        /// </summary>
        public static decimal ToDecimal(object value)
        {
            return value switch
            {
                decimal d => d,
                int i => i,
                long l => l,
                double d => (decimal)d,
                float f => (decimal)f,
                short s => s,
                byte b => b,
                uint u => u,
                ulong ul => ul,
                ushort us => us,
                sbyte sb => sb,
                _ => Convert.ToDecimal(value)
            };
        }

        /// <summary>
        /// Coerces a value to bool? for FEEL three-valued logic.
        /// Returns null if the value is not a boolean.
        /// </summary>
        public static bool? ToBool(object value)
        {
            if (value == null) return null;
            if (value is bool b) return b;
            return null;
        }
    }
}

