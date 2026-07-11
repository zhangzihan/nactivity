using System;

namespace Sys.Rules.DmnEngine.Feel.Types
{
    /// <summary>
    /// FEEL range type representing an interval with endpoints and inclusivity.
    /// Supports open/closed brackets: [a..b], (a..b], [a..b), (a..b).
    /// </summary>
    public sealed class FeelRange
    {
        /// <summary>
        /// Lower bound of the range (null for unbounded)
        /// </summary>
        public object LowEndpoint { get; }

        /// <summary>
        /// Upper bound of the range (null for unbounded)
        /// </summary>
        public object HighEndpoint { get; }

        /// <summary>
        /// Whether the lower bound is inclusive (closed bracket '[')
        /// </summary>
        public bool LowInclusive { get; }

        /// <summary>
        /// Whether the upper bound is inclusive (closed bracket ']')
        /// </summary>
        public bool HighInclusive { get; }

        public FeelRange(object lowEndpoint, bool lowInclusive, object highEndpoint, bool highInclusive)
        {
            LowEndpoint = lowEndpoint;
            HighEndpoint = highEndpoint;
            LowInclusive = lowInclusive;
            HighInclusive = highInclusive;
        }

        /// <summary>
        /// Checks whether the given value falls within this range.
        /// Returns null if comparison is not possible (incompatible types or null value).
        /// </summary>
        public bool? Contains(object value)
        {
            if (value == null) return null;

            var lowOk = CheckLow(value);
            if (lowOk == null) return null;
            if (lowOk == false) return false;

            var highOk = CheckHigh(value);
            if (highOk == null) return null;
            return highOk;
        }

        private bool? CheckLow(object value)
        {
            if (LowEndpoint == null) return true; // unbounded
            var cmp = FeelValueComparer.Compare(value, LowEndpoint);
            if (cmp == null) return null;
            return LowInclusive ? cmp >= 0 : cmp > 0;
        }

        private bool? CheckHigh(object value)
        {
            if (HighEndpoint == null) return true; // unbounded
            var cmp = FeelValueComparer.Compare(value, HighEndpoint);
            if (cmp == null) return null;
            return HighInclusive ? cmp <= 0 : cmp < 0;
        }

        public override string ToString()
        {
            var leftBracket = LowInclusive ? "[" : "(";
            var rightBracket = HighInclusive ? "]" : ")";
            var low = LowEndpoint?.ToString() ?? "";
            var high = HighEndpoint?.ToString() ?? "";
            return $"{leftBracket}{low}..{high}{rightBracket}";
        }

        public override bool Equals(object obj)
        {
            if (obj is not FeelRange other) return false;
            return LowInclusive == other.LowInclusive &&
                   HighInclusive == other.HighInclusive &&
                   Equals(LowEndpoint, other.LowEndpoint) &&
                   Equals(HighEndpoint, other.HighEndpoint);
        }

        public override int GetHashCode() => HashCode.Combine(LowEndpoint, HighEndpoint, LowInclusive, HighInclusive);
    }
}

