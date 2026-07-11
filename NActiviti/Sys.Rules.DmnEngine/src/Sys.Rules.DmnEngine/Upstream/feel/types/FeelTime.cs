using System;

namespace Sys.Rules.DmnEngine.Feel.Types
{
    /// <summary>
    /// FEEL time type wrapping <see cref="TimeOnly"/> with an optional UTC offset.
    /// In FEEL, time values can carry timezone information as an offset from UTC.
    /// </summary>
    public readonly record struct FeelTime : IComparable<FeelTime>, IComparable
    {
        /// <summary>
        /// The time-of-day component
        /// </summary>
        public TimeOnly Time { get; }

        /// <summary>
        /// Optional UTC offset. Null means a "local" time with no timezone.
        /// </summary>
        public TimeSpan? Offset { get; }

        /// <summary>
        /// Whether this time has an explicit timezone offset
        /// </summary>
        public bool HasOffset => Offset.HasValue;

        public FeelTime(TimeOnly time, TimeSpan? offset = null)
        {
            Time = time;
            Offset = offset;
        }

        public FeelTime(int hour, int minute, int second, TimeSpan? offset = null)
            : this(new TimeOnly(hour, minute, second), offset) { }

        public FeelTime(int hour, int minute, int second, int millisecond, TimeSpan? offset = null)
            : this(new TimeOnly(hour, minute, second, millisecond), offset) { }

        /// <summary>
        /// Converts to a <see cref="DateTimeOffset"/> on the epoch date (1970-01-01) for comparison purposes.
        /// </summary>
        public DateTimeOffset ToDateTimeOffset()
        {
            var dt = new DateTime(1970, 1, 1, Time.Hour, Time.Minute, Time.Second, Time.Millisecond, DateTimeKind.Unspecified);
            return new DateTimeOffset(dt, Offset ?? TimeSpan.Zero);
        }

        /// <summary>
        /// Returns the time normalized to UTC for comparison.
        /// </summary>
        private TimeOnly NormalizedUtcTime()
        {
            if (!HasOffset) return Time;
            var dto = ToDateTimeOffset().ToUniversalTime();
            return TimeOnly.FromTimeSpan(dto.TimeOfDay);
        }

        public int CompareTo(FeelTime other)
        {
            // Both local or both with offsets
            if (!HasOffset && !other.HasOffset)
                return Time.CompareTo(other.Time);

            return NormalizedUtcTime().CompareTo(other.NormalizedUtcTime());
        }

        public int CompareTo(object obj)
        {
            if (obj is FeelTime other) return CompareTo(other);
            throw new ArgumentException($"Cannot compare FeelTime with {obj?.GetType().Name ?? "null"}");
        }

        public static bool operator <(FeelTime left, FeelTime right) => left.CompareTo(right) < 0;
        public static bool operator >(FeelTime left, FeelTime right) => left.CompareTo(right) > 0;
        public static bool operator <=(FeelTime left, FeelTime right) => left.CompareTo(right) <= 0;
        public static bool operator >=(FeelTime left, FeelTime right) => left.CompareTo(right) >= 0;

        public static TimeSpan operator -(FeelTime left, FeelTime right)
        {
            return left.ToDateTimeOffset() - right.ToDateTimeOffset();
        }

        public static FeelTime operator +(FeelTime time, TimeSpan duration)
        {
            var newTime = time.Time.Add(duration);
            return new FeelTime(newTime, time.Offset);
        }

        public static FeelTime operator -(FeelTime time, TimeSpan duration)
        {
            return time + (-duration);
        }

        public override string ToString()
        {
            var timeStr = Time.ToString("HH:mm:ss");
            if (!HasOffset) return timeStr;
            if (Offset == TimeSpan.Zero) return $"{timeStr}Z";
            var sign = Offset.Value < TimeSpan.Zero ? "-" : "+";
            var abs = Offset.Value < TimeSpan.Zero ? -Offset.Value : Offset.Value;
            return $"{timeStr}{sign}{abs:hh\\:mm}";
        }
    }
}

