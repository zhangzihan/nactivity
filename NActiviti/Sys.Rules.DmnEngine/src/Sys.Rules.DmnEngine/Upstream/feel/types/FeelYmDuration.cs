using System;

namespace Sys.Rules.DmnEngine.Feel.Types
{
    /// <summary>
    /// FEEL years-and-months duration type.
    /// Represents a duration as a number of years and months (no days/hours/minutes/seconds).
    /// Stored internally as a total number of months.
    /// </summary>
    public readonly record struct FeelYmDuration : IComparable<FeelYmDuration>, IComparable
    {
        /// <summary>
        /// Total number of months (can be negative)
        /// </summary>
        public int TotalMonths { get; }

        /// <summary>
        /// Year component (sign matches TotalMonths)
        /// </summary>
        public int Years => TotalMonths / 12;

        /// <summary>
        /// Month component (sign matches TotalMonths, always 0..11 in absolute value)
        /// </summary>
        public int Months => TotalMonths % 12;

        public FeelYmDuration(int totalMonths)
        {
            TotalMonths = totalMonths;
        }

        public FeelYmDuration(int years, int months)
        {
            TotalMonths = years * 12 + months;
        }

        public static FeelYmDuration operator +(FeelYmDuration left, FeelYmDuration right)
            => new(left.TotalMonths + right.TotalMonths);

        public static FeelYmDuration operator -(FeelYmDuration left, FeelYmDuration right)
            => new(left.TotalMonths - right.TotalMonths);

        public static FeelYmDuration operator -(FeelYmDuration value)
            => new(-value.TotalMonths);

        public static FeelYmDuration operator *(FeelYmDuration duration, int factor)
            => new(duration.TotalMonths * factor);

        public static FeelYmDuration operator *(int factor, FeelYmDuration duration)
            => new(duration.TotalMonths * factor);

        public static FeelYmDuration operator /(FeelYmDuration duration, int divisor)
        {
            if (divisor == 0) throw new DivideByZeroException("Cannot divide duration by zero");
            return new(duration.TotalMonths / divisor);
        }

        /// <summary>
        /// Adds this duration to a date, returning a new date.
        /// </summary>
        public DateOnly AddTo(DateOnly date) => date.AddMonths(TotalMonths);

        /// <summary>
        /// Adds this duration to a date-time, returning a new date-time.
        /// </summary>
        public DateTimeOffset AddTo(DateTimeOffset dateTime) => dateTime.AddMonths(TotalMonths);

        public int CompareTo(FeelYmDuration other) => TotalMonths.CompareTo(other.TotalMonths);

        public int CompareTo(object obj)
        {
            if (obj is FeelYmDuration other) return CompareTo(other);
            throw new ArgumentException($"Cannot compare FeelYmDuration with {obj?.GetType().Name ?? "null"}");
        }

        public static bool operator <(FeelYmDuration left, FeelYmDuration right) => left.TotalMonths < right.TotalMonths;
        public static bool operator >(FeelYmDuration left, FeelYmDuration right) => left.TotalMonths > right.TotalMonths;
        public static bool operator <=(FeelYmDuration left, FeelYmDuration right) => left.TotalMonths <= right.TotalMonths;
        public static bool operator >=(FeelYmDuration left, FeelYmDuration right) => left.TotalMonths >= right.TotalMonths;

        public override string ToString()
        {
            var negative = TotalMonths < 0;
            var abs = Math.Abs(TotalMonths);
            var y = abs / 12;
            var m = abs % 12;
            var prefix = negative ? "-" : "";
            if (y > 0 && m > 0) return $"{prefix}P{y}Y{m}M";
            if (y > 0) return $"{prefix}P{y}Y";
            return $"{prefix}P{m}M";
        }
    }
}

