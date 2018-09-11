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
    private static readonly System.DateTime Jan1st1970 = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
    public static long CurrentUnixTimeMillis()
    {
        return (long)(System.DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
    }

    public static void Add(this DateTime date, DatePart part, int value)
    {
        switch (part)
        {
            case DatePart.ERA:
            case DatePart.YEAR:
                date.AddYears(value);
                break;
            case DatePart.MONTH:
                date.AddMonths(value);
                break;
            case DatePart.DATE:
            case DatePart.DAY:
                date.AddDays(value);
                break;
            case DatePart.HOUR:
            case DatePart.HOUR_OF_DAY:
                date.AddHours(value);
                break;
            case DatePart.MINUTE:
                date.AddMinutes(value);
                break;
            case DatePart.SECOND:
                date.AddSeconds(value);
                break;
            case DatePart.MILLISECOND:
                date.AddMilliseconds(value);
                break;
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