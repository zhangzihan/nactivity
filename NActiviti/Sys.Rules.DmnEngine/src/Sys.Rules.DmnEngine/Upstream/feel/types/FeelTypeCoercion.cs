using System;
using System.Collections.Generic;
using System.Globalization;

namespace Sys.Rules.DmnEngine.Feel.Types
{
    /// <summary>
    /// FEEL-to-CLR and CLR-to-FEEL type conversions.
    /// Handles coercion rules defined by the DMN/FEEL specification.
    /// </summary>
    public static class FeelTypeCoercion
    {
        /// <summary>
        /// Coerces a CLR value to its canonical FEEL representation.
        /// FEEL uses decimal for all numbers, DateOnly for dates, etc.
        /// </summary>
        public static object CoerceToFeel(object value)
        {
            if (value == null) return null;

            return value switch
            {
                // Already canonical FEEL types
                decimal => value,
                string => value,
                bool => value,
                DateOnly => value,
                DateTimeOffset => value,
                TimeSpan => value,
                FeelTime => value,
                FeelYmDuration => value,
                FeelRange => value,
                FeelContext => value,
                FeelFunction => value,
                List<object> => value,

                // Numeric coercion to decimal
                int i => (decimal)i,
                long l => (decimal)l,
                double d => (decimal)d,
                float f => (decimal)f,
                short s => (decimal)s,
                byte b => (decimal)b,
                uint u => (decimal)u,
                ulong ul => (decimal)ul,

                // DateTime 鈫?DateTimeOffset
                DateTime dt => new DateTimeOffset(dt),

                _ => value // pass through unknown types
            };
        }

        /// <summary>
        /// Attempts to coerce a FEEL value to the target CLR type.
        /// Returns null if coercion is not possible.
        /// </summary>
        public static object CoerceToClr(object value, Type targetType)
        {
            if (value == null) return null;
            if (targetType == null) return value;

            // Already the right type
            if (targetType.IsInstanceOfType(value)) return value;

            // Numeric conversions
            if (FeelValueComparer.IsNumeric(value))
            {
                var d = FeelValueComparer.ToDecimal(value);
                if (targetType == typeof(int)) return (int)d;
                if (targetType == typeof(long)) return (long)d;
                if (targetType == typeof(double)) return (double)d;
                if (targetType == typeof(float)) return (float)d;
                if (targetType == typeof(decimal)) return d;
                if (targetType == typeof(short)) return (short)d;
                if (targetType == typeof(byte)) return (byte)d;
            }

            // String parsing
            if (value is string s)
            {
                if (targetType == typeof(decimal) && decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var dec))
                    return dec;
                if (targetType == typeof(int) && int.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var i))
                    return i;
                if (targetType == typeof(long) && long.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var l))
                    return l;
                if (targetType == typeof(double) && double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var dbl))
                    return dbl;
                if (targetType == typeof(bool) && bool.TryParse(s, out var b))
                    return b;
                if (targetType == typeof(DateOnly) && DateOnly.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                    return dt;
                if (targetType == typeof(DateTimeOffset) && DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto))
                    return dto;
            }

            // FEEL singleton list 鈫?scalar coercion
            if (value is List<object> list && list.Count == 1)
                return CoerceToClr(list[0], targetType);

            // Scalar 鈫?singleton list coercion
            if (targetType == typeof(List<object>))
                return new List<object> { value };

            return null;
        }

        /// <summary>
        /// Returns the FEEL type name for a CLR value
        /// </summary>
        public static string GetFeelTypeName(object value)
        {
            if (value == null) return "null";
            return value switch
            {
                decimal => "number",
                int or long or double or float or short or byte => "number",
                string => "string",
                bool => "boolean",
                DateOnly => "date",
                FeelTime => "time",
                DateTimeOffset => "date and time",
                FeelYmDuration => "years and months duration",
                TimeSpan => "days and time duration",
                List<object> => "list",
                FeelContext => "context",
                FeelRange => "range",
                FeelFunction => "function",
                _ => value.GetType().Name
            };
        }
    }
}

