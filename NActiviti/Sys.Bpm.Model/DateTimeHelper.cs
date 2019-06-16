//---------------------------------------------------------------------------------------------------------
//	Copyright © 2007 - 2018 Tangible Software Solutions Inc.
//	This class can be used by anyone provided that the copyright notice remains intact.
//
//	This class is used to replace calls to Java's System.currentTimeMillis with the C# equivalent.
//	Unix time is defined as the number of seconds that have elapsed since midnight UTC, 1 January 1970.
//---------------------------------------------------------------------------------------------------------
using System;

public static class DateTimeHelper
{
    private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public static long CurrentUnixTimeMillis()
    {
        return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
    }

    public static DateTime AddYears(long value)
    {
        return Add(DatePart.YEAR, value);
    }

    public static DateTime AddYears(this DateTime date, long value)
    {
        return date.Add(DatePart.YEAR, value);
    }

    public static DateTime AddMonths(long value)
    {
        return Add(DatePart.MONTH, value);
    }

    public static DateTime AddMonths(this DateTime date, long value)
    {
        return date.Add(DatePart.MONTH, value);
    }

    public static DateTime AddDays(long value)
    {
        return Add(DatePart.DAY, value);
    }

    public static DateTime AddDays(this DateTime date, long value)
    {
        return date.Add(DatePart.DAY, value);
    }

    public static DateTime AddWeeks(long value)
    {
        return Add(DatePart.WEEK, value);
    }

    public static DateTime AddWeeks(this DateTime date, long value)
    {
        return date.Add(DatePart.WEEK, value);
    }

    public static DateTime AddHours(long value)
    {
        return Add(DatePart.HOUR, value);
    }

    public static DateTime AddHours(this DateTime date, long value)
    {
        return date.Add(DatePart.HOUR, value);
    }

    public static DateTime AddMinutes(long value)
    {
        return Add(DatePart.MINUTE, value);
    }

    public static DateTime AddMinutes(this DateTime date, long value)
    {
        return date.Add(DatePart.MINUTE, value);
    }

    public static DateTime AddSeconds(long value)
    {
        return Add(DatePart.SECOND, value);
    }

    public static DateTime AddSeconds(this DateTime date, long value)
    {
        return date.Add(DatePart.SECOND, value);
    }

    public static DateTime Add(DatePart part, long value)
    {
        return DateTime.Now.Add(part, value);
    }

    public static DateTime Add(this DateTime date, DatePart part, long value)
    {
        return Add(date, part, (int)value);
    }

    public static DateTime Set(this DateTime date, DatePart part, long value)
    {
        return Set(date, part, (int)value);
    }

    public static DateTime AddYears(int value)
    {
        return Add(DatePart.YEAR, value);
    }

    public static DateTime AddYears(this DateTime date, int value)
    {
        return date.Add(DatePart.YEAR, value);
    }

    public static DateTime AddMonths(int value)
    {
        return Add(DatePart.MONTH, value);
    }

    public static DateTime AddMonths(this DateTime date, int value)
    {
        return date.Add(DatePart.MONTH, value);
    }

    public static DateTime AddDays(int value)
    {
        return Add(DatePart.DAY, value);
    }

    public static DateTime AddDays(this DateTime date, int value)
    {
        return date.Add(DatePart.DAY, value);
    }

    public static DateTime AddWeeks(int value)
    {
        return Add(DatePart.WEEK, value);
    }

    public static DateTime AddWeeks(this DateTime date, int value)
    {
        return date.Add(DatePart.WEEK, value);
    }

    public static DateTime AddHours(int value)
    {
        return Add(DatePart.HOUR, value);
    }

    public static DateTime AddHours(this DateTime date, int value)
    {
        return date.Add(DatePart.HOUR, value);
    }

    public static DateTime AddMinutes(int value)
    {
        return Add(DatePart.MINUTE, value);
    }

    public static DateTime AddMinutes(this DateTime date, int value)
    {
        return date.Add(DatePart.MINUTE, value);
    }

    public static DateTime AddSeconds(int value)
    {
        return Add(DatePart.SECOND, value);
    }

    public static DateTime AddSeconds(this DateTime date, int value)
    {
        return date.Add(DatePart.SECOND, value);
    }

    public static DateTime Add(DatePart part, int value)
    {
        return DateTime.Now.Add(part, value);
    }

    public static DateTime Add(this DateTime date, DatePart part, int value)
    {
        switch (part)
        {
            case DatePart.ERA:
            case DatePart.YEAR:
                return date.AddYears(value);
            case DatePart.MONTH:
                return date.AddMonths(value);
            case DatePart.DATE:
            case DatePart.DAY:
                return date.AddDays(value);
            case DatePart.HOUR:
            case DatePart.HOUR_OF_DAY:
                return date.AddHours(value);
            case DatePart.MINUTE:
                return date.AddMinutes(value);
            case DatePart.SECOND:
                return date.AddSeconds(value);
            case DatePart.MILLISECOND:
                return date.AddMilliseconds(value);
            case DatePart.WEEK:
                return date.AddDays(value * 7);
            default:
                return date;
        }
    }

    public static DateTime Set(this DateTime date, DatePart part, int value)
    {
        switch (part)
        {
            case DatePart.YEAR:
                return new DateTime(value, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Millisecond);
            case DatePart.MONTH:
                return new DateTime(date.Year, value, date.Day, date.Hour, date.Minute, date.Second, date.Millisecond);
            case DatePart.DAY:
                return new DateTime(date.Year, date.Month, value, date.Hour, date.Minute, date.Second, date.Millisecond);
            case DatePart.HOUR:
                return new DateTime(date.Year, date.Month, date.Day, value, date.Minute, date.Second, date.Millisecond);
            case DatePart.MINUTE:
                return new DateTime(date.Year, date.Month, date.Day, date.Hour, value, date.Second, date.Millisecond);
            case DatePart.SECOND:
                return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, value, date.Millisecond);
            case DatePart.MILLISECOND:
                return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, value);
            default:
                return date;
        }
    }
}

public enum DatePart
{
    ERA = 0,
    YEAR = 1,
    MONTH = 2,
    WEEK = 3,
    WEEK_OF_YEAR = 3,
    WEEK_OF_MONTH = 4,
    DATE = 5,
    DAY_OF_MONTH = 5,
    DAY_OF_YEAR = 6,
    DAY_OF_WEEK = 7,
    DAY = 8,
    DAY_OF_WEEK_IN_MONTH = 8,
    AM_PM = 9,
    HOUR = 10,
    HOUR_OF_DAY = 11,
    MINUTE = 12,
    SECOND = 13,
    MILLISECOND = 14,
    ZONE_OFFSET = 15,
    DST_OFFSET = 16,
    FIELD_COUNT = 17,
    SUNDAY = 1,
    MONDAY = 2,
    TUESDAY = 3,
    WEDNESDAY = 4,
    THURSDAY = 5,
    FRIDAY = 6,
    SATURDAY = 7,
    JANUARY = 0,
    FEBRUARY = 1,
    MARCH = 2,
    APRIL = 3,
    MAY = 4,
    JUNE = 5,
    JULY = 6,
    AUGUST = 7,
    SEPTEMBER = 8,
    OCTOBER = 9,
    NOVEMBER = 10,
    DECEMBER = 11,
    UNDECIMBER = 12,
    AM = 0,
    PM = 1
}