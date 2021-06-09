using System;
using System.Collections.Generic;
using System.Text;

namespace Sys.Workflow.Engine.Impl.Calendars
{
    /*
	 * Included from org.quartz-scheduler/quartz/1.8.4
	 * All content copyright Terracotta, Inc., unless otherwise indicated. All rights reserved.
	 *
	 * Licensed under the Apache License, Version 2.0 (the "License"); you may not
	 * use this file except in compliance with the License. You may obtain a copy
	 * of the License at
	 *
	 *   http://www.apache.org/licenses/LICENSE-2.0
	 *
	 * Unless required by applicable law or agreed to in writing, software
	 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
	 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
	 * License for the specific language governing permissions and limitations
	 * under the License.
	 *
	 */


    using Sys.Workflow.Engine.Runtime;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Provides a parser and evaluator for unix-like cron expressions. Cron expressions provide the ability to specify complex time combinations such as &quot;At 8:00am every Monday through Friday&quot;
    /// or &quot;At 1:30am every last Friday of the month&quot;.
    /// <P>
    /// Cron expressions are comprised of 6 required fields and one optional field separated by white space. The fields respectively are described as follows:
    /// 
    /// <table cellspacing="8">
    /// <tr>
    /// <th align="left">Field Name</th>
    /// <th align="left">&nbsp;</th>
    /// <th align="left">Allowed Values</th>
    /// <th align="left">&nbsp;</th>
    /// <th align="left">Allowed Special Characters</th>
    /// </tr>
    /// <tr>
    /// <td align="left"><code>Seconds</code></td>
    /// <td align="left">&nbsp;</th>
    /// <td align="left"><code>0-59</code></td>
    /// <td align="left">&nbsp;</th>
    /// <td align="left"><code>, - * /</code></td>
    /// </tr>
    /// <tr>
    /// <td align="left"><code>Minutes</code></td>
    /// <td align="left">&nbsp;</th>
    /// <td align="left"><code>0-59</code></td>
    /// <td align="left">&nbsp;</th>
    /// <td align="left"><code>, - * /</code></td>
    /// </tr>
    /// <tr>
    /// <td align="left"><code>Hours</code></td>
    /// <td align="left">&nbsp;</th>
    /// <td align="left"><code>0-23</code></td>
    /// <td align="left">&nbsp;</th>
    /// <td align="left"><code>, - * /</code></td>
    /// </tr>
    /// <tr>
    /// <td align="left"><code>Day-of-month</code></td>
    /// <td align="left">&nbsp;</th>
    /// <td align="left"><code>1-31</code></td>
    /// <td align="left">&nbsp;</th>
    /// <td align="left"><code>, - * ? / L W</code></td>
    /// </tr>
    /// <tr>
    /// <td align="left"><code>Month</code></td>
    /// <td align="left">&nbsp;</th>
    /// <td align="left"><code>1-12 or JAN-DEC</code></td>
    /// <td align="left">&nbsp;</th>
    /// <td align="left"><code>, - * /</code></td>
    /// </tr>
    /// <tr>
    /// <td align="left"><code>Day-of-Week</code></td>
    /// <td align="left">&nbsp;</th>
    /// <td align="left"><code>1-7 or SUN-SAT</code></td>
    /// <td align="left">&nbsp;</th>
    /// <td align="left"><code>, - * ? / L #</code></td>
    /// </tr>
    /// <tr>
    /// <td align="left"><code>Year (Optional)</code></td>
    /// <td align="left">&nbsp;</th>
    /// <td align="left"><code>empty, 1970-2199</code></td>
    /// <td align="left">&nbsp;</th>
    /// <td align="left"><code>, - * /</code></td>
    /// </tr>
    /// </table>
    /// <P>
    /// The '*' character is used to specify all values. For example, &quot;*&quot; in the minute field means &quot;every minute&quot;.
    /// <P>
    /// The '?' character is allowed for the day-of-month and day-of-week fields. It is used to specify 'no specific value'. This is useful when you need to specify something in one of the two fields, but
    /// not the other.
    /// <P>
    /// The '-' character is used to specify ranges For example &quot;10-12&quot; in the hour field means &quot;the hours 10, 11 and 12&quot;.
    /// <P>
    /// The ',' character is used to specify additional values. For example &quot;MON,WED,FRI&quot; in the day-of-week field means &quot;the days Monday, Wednesday, and Friday&quot;.
    /// <P>
    /// The '/' character is used to specify increments. For example &quot;0/15&quot; in the seconds field means &quot;the seconds 0, 15, 30, and 45&quot;. And &quot;5/15&quot; in the seconds field means
    /// &quot;the seconds 5, 20, 35, and 50&quot;. Specifying '*' before the '/' is equivalent to specifying 0 is the value to start with. Essentially, for each field in the expression, there is a set of
    /// numbers that can be turned on or off. For seconds and minutes, the numbers range from 0 to 59. For hours 0 to 23, for days of the month 0 to 31, and for months 1 to 12. The &quot;/&quot; character
    /// simply helps you turn on every &quot;nth&quot; value in the given set. Thus &quot;7/6&quot; in the month field only turns on month &quot;7&quot;, it does NOT mean every 6th month, please note that
    /// subtlety.
    /// <P>
    /// The 'L' character is allowed for the day-of-month and day-of-week fields. This character is short-hand for &quot;last&quot;, but it has different meaning in each of the two fields. For example, the
    /// value &quot;L&quot; in the day-of-month field means &quot;the last day of the month&quot; - day 31 for January, day 28 for February on non-leap years. If used in the day-of-week field by itself, it
    /// simply means &quot;7&quot; or &quot;SAT&quot;. But if used in the day-of-week field after another value, it means &quot;the last xxx day of the month&quot; - for example &quot;6L&quot; means
    /// &quot;the last friday of the month&quot;. You can also specify an offset from the last day of the month, such as "L-3" which would mean the third-to-last day of the calendar month. <i>When using
    /// the 'L' option, it is important not to specify lists, or ranges of values, as you'll get confusing/unexpected results.</i>
    /// <P>
    /// The 'W' character is allowed for the day-of-month field. This character is used to specify the weekday (Monday-Friday) nearest the given day. As an example, if you were to specify &quot;15W&quot;
    /// as the value for the day-of-month field, the meaning is: &quot;the nearest weekday to the 15th of the month&quot;. So if the 15th is a Saturday, the trigger will fire on Friday the 14th. If the
    /// 15th is a Sunday, the trigger will fire on Monday the 16th. If the 15th is a Tuesday, then it will fire on Tuesday the 15th. However if you specify &quot;1W&quot; as the value for day-of-month, and
    /// the 1st is a Saturday, the trigger will fire on Monday the 3rd, as it will not 'jump' over the boundary of a month's days. The 'W' character can only be specified when the day-of-month is a single
    /// day, not a range or list of days.
    /// <P>
    /// The 'L' and 'W' characters can also be combined for the day-of-month expression to yield 'LW', which translates to &quot;last weekday of the month&quot;.
    /// <P>
    /// The '#' character is allowed for the day-of-week field. This character is used to specify &quot;the nth&quot; XXX day of the month. For example, the value of &quot;6#3&quot; in the day-of-week
    /// field means the third Friday of the month (day 6 = Friday and &quot;#3&quot; = the 3rd one in the month). Other examples: &quot;2#1&quot; = the first Monday of the month and &quot;4#5&quot; = the
    /// fifth Wednesday of the month. Note that if you specify &quot;#5&quot; and there is not 5 of the given day-of-week in the month, then no firing will occur that month. If the '#' character is used,
    /// there can only be one expression in the day-of-week field (&quot;3#1,6#3&quot; is not valid, since there are two expressions).
    /// <P>
    /// <!--The 'C' character is allowed for the day-of-month and day-of-week fields. This character is short-hand for "calendar". This means values are calculated against the associated calendar, if any.
    /// If no calendar is associated, then it is equivalent to having an all-inclusive calendar. A value of "5C" in the day-of-month field means "the first day included by the calendar on or after the
    /// 5th". A value of "1C" in the day-of-week field means "the first day included by the calendar on or after Sunday".-->
    /// <P>
    /// The legal characters and the names of months and days of the week are not case sensitive.
    /// 
    /// <para>
    /// <b>NOTES:</b>
    /// <ul>
    /// <li>Support for specifying both a day-of-week and a day-of-month value is not complete (you'll need to use the '?' character in one of these fields).</li>
    /// <li>Overflowing ranges is supported - that is, having a larger number on the left hand side than the right. You might do 22-2 to catch 10 o'clock at night until 2 o'clock in the morning, or you
    /// might have NOV-FEB. It is very important to note that overuse of overflowing ranges creates ranges that don't make sense and no effort has been made to determine which interpretation CronExpression
    /// chooses. An example would be "0 0 14-6 ? * FRI-MON".</li>
    /// </ul>
    /// </para>
    /// 
    /// 
    /// 
    /// 
    /// 
    /// </summary>
    [Serializable]
    public class CronExpression : ICloneable
    {

        private const long serialVersionUID = 12423409423L;

        protected internal const int SECOND = 0;
        protected internal const int MINUTE = 1;
        protected internal const int HOUR = 2;
        protected internal const int DAY_OF_MONTH = 3;
        protected internal const int MONTH = 4;
        protected internal const int DAY_OF_WEEK = 5;
        protected internal const int YEAR = 6;
        protected internal const int ALL_SPEC_INT = 99; // '*'
        protected internal const int NO_SPEC_INT = 98; // '?'
        protected internal static readonly int? ALL_SPEC = Convert.ToInt32(ALL_SPEC_INT);
        protected internal static readonly int? NO_SPEC = Convert.ToInt32(NO_SPEC_INT);

        protected internal static readonly IDictionary<string, int> monthMap = new Dictionary<string, int>(20);
        protected internal static readonly IDictionary<string, int> dayMap = new Dictionary<string, int>(60);
        static CronExpression()
        {
            monthMap["JAN"] = Convert.ToInt32(0);
            monthMap["FEB"] = Convert.ToInt32(1);
            monthMap["MAR"] = Convert.ToInt32(2);
            monthMap["APR"] = Convert.ToInt32(3);
            monthMap["MAY"] = Convert.ToInt32(4);
            monthMap["JUN"] = Convert.ToInt32(5);
            monthMap["JUL"] = Convert.ToInt32(6);
            monthMap["AUG"] = Convert.ToInt32(7);
            monthMap["SEP"] = Convert.ToInt32(8);
            monthMap["OCT"] = Convert.ToInt32(9);
            monthMap["NOV"] = Convert.ToInt32(10);
            monthMap["DEC"] = Convert.ToInt32(11);

            dayMap["SUN"] = Convert.ToInt32(1);
            dayMap["MON"] = Convert.ToInt32(2);
            dayMap["TUE"] = Convert.ToInt32(3);
            dayMap["WED"] = Convert.ToInt32(4);
            dayMap["THU"] = Convert.ToInt32(5);
            dayMap["FRI"] = Convert.ToInt32(6);
            dayMap["SAT"] = Convert.ToInt32(7);
        }

        private readonly string cronExpression;
        private TimeZoneInfo timeZone;
        [NonSerialized]
        protected internal SortedSet<int> seconds;
        [NonSerialized]
        protected internal SortedSet<int> minutes;
        [NonSerialized]
        protected internal SortedSet<int> hours;
        [NonSerialized]
        protected internal SortedSet<int> daysOfMonth;
        [NonSerialized]
        protected internal SortedSet<int> months;
        [NonSerialized]
        protected internal SortedSet<int> daysOfWeek;
        [NonSerialized]
        protected internal SortedSet<int> years;

        [NonSerialized]
        protected internal bool lastdayOfWeek;
        [NonSerialized]
        protected internal int nthdayOfWeek;
        [NonSerialized]
        protected internal bool lastdayOfMonth;
        [NonSerialized]
        protected internal bool nearestWeekday;
        [NonSerialized]
        protected internal int lastdayOffset;
        [NonSerialized]
        protected internal bool expressionParsed;

        public static readonly int MAX_YEAR = new DateTime().Year + 100;

        private readonly IClockReader clockReader;

        /// <summary>
        /// Constructs a new <CODE>CronExpression</CODE> based on the specified parameter.
        /// </summary>
        /// <param name="cronExpression">
        ///          String representation of the cron expression the new object should represent </param>
        /// <param name="clockReader">
        ///          The reader which will provide the current time </param>
        /// <param name="timeZone">
        ///          The time zone that will be used for calculations
        /// </param>
        /// <exception cref="java.text.Exception">
        public CronExpression(string cronExpression, IClockReader clockReader, TimeZoneInfo timeZone) : this(cronExpression, clockReader)
        {
            this.timeZone = timeZone;
        }

        /// <summary>
        /// Constructs a new <CODE>CronExpression</CODE> based on the specified parameter.
        /// </summary>
        /// <param name="cronExpression">
        ///          String representation of the cron expression the new object should represent </param>
        /// <exception cref="java.text.Exception">
        ///           if the string expression cannot be parsed into a valid <CODE>CronExpression</CODE> </exception>
        public CronExpression(string cronExpression, IClockReader clockReader)
        {
            if (cronExpression is null)
            {
                throw new System.ArgumentException("cronExpression cannot be null");
            }

            this.clockReader = clockReader;
            this.cronExpression = cronExpression.ToUpper(CultureInfo.GetCultureInfo("en-us"));

            BuildExpression(this.cronExpression);
        }

        /// <summary>
        /// Returns the time zone for which this <code>CronExpression</code> will be resolved.
        /// </summary>
        public virtual TimeZoneInfo TimeZone
        {
            get
            {
                if (timeZone is null)
                {
                    timeZone = clockReader.CurrentTimeZone;
                }

                return timeZone;
            }
            set
            {
                this.timeZone = value;
            }
        }


        /// <summary>
        /// Returns the string representation of the <CODE>CronExpression</CODE>
        /// </summary>
        /// <returns> a string representation of the <CODE>CronExpression</CODE> </returns>
        public override string ToString()
        {
            return cronExpression;
        }

        // //////////////////////////////////////////////////////////////////////////
        //
        // Expression Parsing Functions
        //
        // //////////////////////////////////////////////////////////////////////////

        protected internal virtual void BuildExpression(string expression)
        {
            expressionParsed = true;

            try
            {

                if (seconds is null)
                {
                    seconds = new SortedSet<int>();
                }
                if (minutes is null)
                {
                    minutes = new SortedSet<int>();
                }
                if (hours is null)
                {
                    hours = new SortedSet<int>();
                }
                if (daysOfMonth is null)
                {
                    daysOfMonth = new SortedSet<int>();
                }
                if (months is null)
                {
                    months = new SortedSet<int>();
                }
                if (daysOfWeek is null)
                {
                    daysOfWeek = new SortedSet<int>();
                }
                if (years is null)
                {
                    years = new SortedSet<int>();
                }

                int exprOn = SECOND;

                string[] exprsTok = expression.Split(" \t", false);

                for (var idx = 0; idx < exprsTok.Length && exprOn <= YEAR; idx++)
                {
                    string expr = exprsTok[idx].Trim();

                    // throw an exception if L is used with other days of the month
                    if (exprOn == DAY_OF_MONTH && expr.IndexOf('L') != -1 && expr.Length > 1 && expr.IndexOf(",", StringComparison.Ordinal) >= 0)
                    {
                        throw new Exception("Support for specifying 'L' and 'LW' with other days of the month is not implemented -1");
                    }
                    // throw an exception if L is used with other days of the week
                    if (exprOn == DAY_OF_WEEK && expr.IndexOf('L') != -1 && expr.Length > 1 && expr.IndexOf(",", StringComparison.Ordinal) >= 0)
                    {
                        throw new Exception("Support for specifying 'L' with other days of the week is not implemented -1");
                    }
                    if (exprOn == DAY_OF_WEEK && expr.IndexOf('#') != -1 && expr.IndexOf('#', expr.IndexOf('#') + 1) != -1)
                    {
                        throw new Exception("Support for specifying multiple \"nth\" days is not implemented.-1");
                    }

                    string[] vTok = expr.Split(',');
                    foreach (var v in vTok)
                    {
                        StoreExpressionVals(0, v, exprOn);
                    }

                    exprOn++;
                }

                if (exprOn <= DAY_OF_WEEK)
                {
                    throw new Exception($"Unexpected end of expression. {expression.Length}");
                }

                if (exprOn <= YEAR)
                {
                    StoreExpressionVals(0, "*", YEAR);
                }

                SortedSet<int> dow = GetSet(DAY_OF_WEEK);
                SortedSet<int> dom = GetSet(DAY_OF_MONTH);

                // Copying the logic from the UnsupportedOperationException below
                bool dayOfMSpec = !dom.Contains(NO_SPEC.Value);
                bool dayOfWSpec = !dow.Contains(NO_SPEC.Value);

                if (dayOfMSpec && !dayOfWSpec)
                {
                    // skip
                }
                else if (dayOfWSpec && !dayOfMSpec)
                {
                    // skip
                }
                else
                {
                    throw new Exception("Support for specifying both a day-of-week AND a day-of-month parameter is not implemented. 0");
                }
            }
            catch (Exception pe)
            {
                throw new Exception("Illegal cron expression format (" + pe.ToString() + ")", pe);
            }
        }

        protected internal virtual int StoreExpressionVals(int pos, string s, int type)
        {
            int incr = 0;
            int i = SkipWhiteSpace(pos, s);
            if (i >= s.Length)
            {
                return i;
            }
            char c = s[i];
            if ((c >= 'A') && (c <= 'Z') && (!s.Equals("L")) && (!s.Equals("LW")) && (!new Regex("^L-[0-9]*[W]?").IsMatch(s)))
            {
                string sub = s.Substring(i, 3);
                int eval = -1;
                int sval;
                if (type == MONTH)
                {
                    sval = GetMonthNumber(sub) + 1;
                    if (sval <= 0)
                    {
                        throw new Exception($"Invalid Month value: '{sub}' {i}");
                    }
                    if (s.Length > i + 3)
                    {
                        c = s[i + 3];
                        if (c == '-')
                        {
                            i += 4;
                            sub = s.Substring(i, 3);
                            eval = GetMonthNumber(sub) + 1;
                            if (eval <= 0)
                            {
                                throw new Exception($"Invalid Month value: '{sub}'{i}");
                            }
                        }
                    }
                }
                else if (type == DAY_OF_WEEK)
                {
                    sval = GetDayOfWeekNumber(sub);
                    if (sval < 0)
                    {
                        throw new Exception($"Invalid Day-of-Week value: '{sub}'{i}");
                    }
                    if (s.Length > i + 3)
                    {
                        c = s[i + 3];
                        if (c == '-')
                        {
                            i += 4;
                            sub = s.Substring(i, 3);
                            eval = GetDayOfWeekNumber(sub);
                            if (eval < 0)
                            {
                                throw new Exception($"Invalid Day-of-Week value: '{sub}'{i}");
                            }
                        }
                        else if (c == '#')
                        {
                            try
                            {
                                i += 4;
                                nthdayOfWeek = int.Parse(s.Substring(i));
                                if (nthdayOfWeek < 1 || nthdayOfWeek > 5)
                                {
                                    throw new Exception();
                                }
                            }
                            catch (Exception)
                            {
                                throw new Exception($"A numeric value between 1 and 5 must follow the '#' option{i}");
                            }
                        }
                        else if (c == 'L')
                        {
                            lastdayOfWeek = true;
                            i++;
                        }
                    }

                }
                else
                {
                    throw new Exception("Illegal characters for this position: '{sub}'{i}");
                }
                if (eval != -1)
                {
                    incr = 1;
                }
                AddToSet(sval, eval, incr, type);
                return (i + 3);
            }

            if (c == '?')
            {
                i++;
                if ((i + 1) < s.Length && (s[i] != ' ' && s[i + 1] != '\t'))
                {
                    throw new Exception($"Illegal character after '?': {s[i]} {i}");
                }
                if (type != DAY_OF_WEEK && type != DAY_OF_MONTH)
                {
                    throw new Exception("'?' can only be specified for Day-of-Month or Day-of-Week." + i);
                }
                if (type == DAY_OF_WEEK && !lastdayOfMonth)
                {
                    int val = ((int?)daysOfMonth.Max).Value;
                    if (val == NO_SPEC_INT)
                    {
                        throw new Exception("'?' can only be specified for Day-of-Month -OR- Day-of-Week." + i);
                    }
                }

                AddToSet(NO_SPEC_INT, -1, 0, type);
                return i;
            }

            if (c == '*' || c == '/')
            {
                if (c == '*' && (i + 1) >= s.Length)
                {
                    AddToSet(ALL_SPEC_INT, -1, incr, type);
                    return i + 1;
                }
                else if (c == '/' && ((i + 1) >= s.Length || s[i + 1] == ' ' || s[i + 1] == '\t'))
                {
                    throw new Exception("'/' must be followed by an integer." + i);
                }
                else if (c == '*')
                {
                    i++;
                }
                c = s[i];
                if (c == '/')
                { // is an increment specified?
                    i++;
                    if (i >= s.Length)
                    {
                        throw new Exception("Unexpected end of string." + i);
                    }

                    incr = GetNumericValue(s, i);

                    i++;
                    if (incr > 10)
                    {
                        i++;
                    }
                    if (incr > 59 && (type == SECOND || type == MINUTE))
                    {
                        throw new Exception("Increment > 60 : " + incr + i);
                    }
                    else if (incr > 23 && (type == HOUR))
                    {
                        throw new Exception("Increment > 24 : " + incr + i);
                    }
                    else if (incr > 31 && (type == DAY_OF_MONTH))
                    {
                        throw new Exception("Increment > 31 : " + incr + i);
                    }
                    else if (incr > 7 && (type == DAY_OF_WEEK))
                    {
                        throw new Exception("Increment > 7 : " + incr + i);
                    }
                    else if (incr > 12 && (type == MONTH))
                    {
                        throw new Exception("Increment > 12 : " + incr + i);
                    }
                }
                else
                {
                    incr = 1;
                }

                AddToSet(ALL_SPEC_INT, -1, incr, type);
                return i;
            }
            else if (c == 'L')
            {
                i++;
                if (type == DAY_OF_MONTH)
                {
                    lastdayOfMonth = true;
                }
                if (type == DAY_OF_WEEK)
                {
                    AddToSet(7, 7, 0, type);
                }
                if (type == DAY_OF_MONTH && s.Length > i)
                {
                    c = s[i];
                    if (c == '-')
                    {
                        ValueSet vs = GetValue(0, s, i + 1);
                        lastdayOffset = vs.value;
                        if (lastdayOffset > 30)
                        {
                            throw new Exception("Offset from last day must be <= 30" + (i + 1));
                        }
                        i = vs.pos;
                    }
                    if (s.Length > i)
                    {
                        c = s[i];
                        if (c == 'W')
                        {
                            nearestWeekday = true;
                            i++;
                        }
                    }
                }
                return i;
            }
            else if (c >= '0' && c <= '9')
            {
                int val = int.Parse(c.ToString());
                i++;
                if (i >= s.Length)
                {
                    AddToSet(val, -1, -1, type);
                }
                else
                {
                    c = s[i];
                    if (c >= '0' && c <= '9')
                    {
                        ValueSet vs = GetValue(val, s, i);
                        val = vs.value;
                        i = vs.pos;
                    }
                    i = CheckNext(i, s, val, type);
                    return i;
                }
            }
            else
            {
                throw new Exception("Unexpected character: " + c + i);
            }

            return i;
        }

        protected internal virtual int CheckNext(int pos, string s, int val, int type)
        {

            int end = -1;
            int i = pos;

            if (i >= s.Length)
            {
                AddToSet(val, end, -1, type);
                return i;
            }

            char c = s[pos];

            if (c == 'L')
            {
                if (type == DAY_OF_WEEK)
                {
                    if (val < 1 || val > 7)
                    {
                        throw new Exception("Day-of-Week values must be between 1 and 7" + -1);
                    }
                    lastdayOfWeek = true;
                }
                else
                {
                    throw new Exception("'L' option is not valid here. (pos=" + i + ")" + i);
                }
                SortedSet<int> set = GetSet(type);
                set.Add(Convert.ToInt32(val));
                i++;
                return i;
            }

            if (c == 'W')
            {
                if (type == DAY_OF_MONTH)
                {
                    nearestWeekday = true;
                }
                else
                {
                    throw new Exception("'W' option is not valid here. (pos=" + i + ") " + i);
                }
                if (val > 31)
                {
                    throw new Exception("The 'W' option does not make sense with values larger than 31 (max number of days in a month)" + i);
                }
                SortedSet<int> set = GetSet(type);
                set.Add(Convert.ToInt32(val));
                i++;
                return i;
            }

            if (c == '#')
            {
                if (type != DAY_OF_WEEK)
                {
                    throw new Exception("'#' option is not valid here. (pos=" + i + ") " + i);
                }
                i++;
                try
                {
                    nthdayOfWeek = int.Parse(s.Substring(i));
                    if (nthdayOfWeek < 1 || nthdayOfWeek > 5)
                    {
                        throw new Exception();
                    }
                }
                catch (Exception)
                {
                    throw new Exception("A numeric value between 1 and 5 must follow the '#' option " + i);
                }

                SortedSet<int> set = GetSet(type);
                set.Add(Convert.ToInt32(val));
                i++;
                return i;
            }

            if (c == '-')
            {
                i++;
                c = s[i];
                int v = int.Parse(c.ToString());
                end = v;
                i++;
                if (i >= s.Length)
                {
                    AddToSet(val, end, 1, type);
                    return i;
                }
                c = s[i];
                if (c >= '0' && c <= '9')
                {
                    ValueSet vs = GetValue(v, s, i);
                    int v1 = vs.value;
                    end = v1;
                    i = vs.pos;
                }
                if (i < s.Length && ((_ = s[i]) == '/'))
                {
                    i++;
                    c = s[i];
                    int v2 = int.Parse(c.ToString());
                    i++;
                    if (i >= s.Length)
                    {
                        AddToSet(val, end, v2, type);
                        return i;
                    }
                    c = s[i];
                    if (c >= '0' && c <= '9')
                    {
                        ValueSet vs = GetValue(v2, s, i);
                        int v3 = vs.value;
                        AddToSet(val, end, v3, type);
                        i = vs.pos;
                        return i;
                    }
                    else
                    {
                        AddToSet(val, end, v2, type);
                        return i;
                    }
                }
                else
                {
                    AddToSet(val, end, 1, type);
                    return i;
                }
            }

            if (c == '/')
            {
                i++;
                c = s[i];
                int v2 = int.Parse(c.ToString());
                i++;
                if (i >= s.Length)
                {
                    AddToSet(val, end, v2, type);
                    return i;
                }
                c = s[i];
                if (c >= '0' && c <= '9')
                {
                    ValueSet vs = GetValue(v2, s, i);
                    int v3 = vs.value;
                    AddToSet(val, end, v3, type);
                    i = vs.pos;
                    return i;
                }
                else
                {
                    throw new Exception("Unexpected character '" + c + "' after '/' " + i);
                }
            }

            AddToSet(val, end, 0, type);
            i++;
            return i;
        }

        protected internal virtual int SkipWhiteSpace(int i, string s)
        {
            for (; i < s.Length && (s[i] == ' ' || s[i] == '\t'); i++)
            {
            }

            return i;
        }

        protected internal virtual int FindNextWhiteSpace(int i, string s)
        {
            for (; i < s.Length && (s[i] != ' ' || s[i] != '\t'); i++)
            {
            }

            return i;
        }

        protected internal virtual void AddToSet(int val, int end, int incr, int type)
        {
            SortedSet<int> set = GetSet(type);

            if (type == SECOND || type == MINUTE)
            {
                if ((val < 0 || val > 59 || end > 59) && (val != ALL_SPEC_INT))
                {
                    throw new Exception("Minute and Second values must be between 0 and 59" + -1);
                }
            }
            else if (type == HOUR)
            {
                if ((val < 0 || val > 23 || end > 23) && (val != ALL_SPEC_INT))
                {
                    throw new Exception("Hour values must be between 0 and 23" + -1);
                }
            }
            else if (type == DAY_OF_MONTH)
            {
                if ((val < 1 || val > 31 || end > 31) && (val != ALL_SPEC_INT) && (val != NO_SPEC_INT))
                {
                    throw new Exception("Day of month values must be between 1 and 31" + -1);
                }
            }
            else if (type == MONTH)
            {
                if ((val < 1 || val > 12 || end > 12) && (val != ALL_SPEC_INT))
                {
                    throw new Exception("Month values must be between 1 and 12" + -1);
                }
            }
            else if (type == DAY_OF_WEEK)
            {
                if ((val == 0 || val > 7 || end > 7) && (val != ALL_SPEC_INT) && (val != NO_SPEC_INT))
                {
                    throw new Exception("Day-of-Week values must be between 1 and 7" + -1);
                }
            }

            if ((incr == 0 || incr == -1) && val != ALL_SPEC_INT)
            {
                if (val != -1)
                {
                    set.Add(Convert.ToInt32(val));
                }
                else
                {
                    set.Add(NO_SPEC.Value);
                }

                return;
            }

            int startAt = val;
            int stopAt = end;

            if (val == ALL_SPEC_INT && incr <= 0)
            {
                incr = 1;
                set.Add(ALL_SPEC.Value); // put in a marker, but also fill values
            }

            if (type == SECOND || type == MINUTE)
            {
                if (stopAt == -1)
                {
                    stopAt = 59;
                }
                if (startAt == -1 || startAt == ALL_SPEC_INT)
                {
                    startAt = 0;
                }
            }
            else if (type == HOUR)
            {
                if (stopAt == -1)
                {
                    stopAt = 23;
                }
                if (startAt == -1 || startAt == ALL_SPEC_INT)
                {
                    startAt = 0;
                }
            }
            else if (type == DAY_OF_MONTH)
            {
                if (stopAt == -1)
                {
                    stopAt = 31;
                }
                if (startAt == -1 || startAt == ALL_SPEC_INT)
                {
                    startAt = 1;
                }
            }
            else if (type == MONTH)
            {
                if (stopAt == -1)
                {
                    stopAt = 12;
                }
                if (startAt == -1 || startAt == ALL_SPEC_INT)
                {
                    startAt = 1;
                }
            }
            else if (type == DAY_OF_WEEK)
            {
                if (stopAt == -1)
                {
                    stopAt = 7;
                }
                if (startAt == -1 || startAt == ALL_SPEC_INT)
                {
                    startAt = 1;
                }
            }
            else if (type == YEAR)
            {
                if (stopAt == -1)
                {
                    stopAt = MAX_YEAR;
                }
                if (startAt == -1 || startAt == ALL_SPEC_INT)
                {
                    startAt = 1970;
                }
            }

            // if the end of the range is before the start, then we need to overflow
            // into
            // the next day, month etc. This is done by adding the maximum amount
            // for
            // that
            // type, and using modulus max to determine the value being added.
            int max = -1;
            if (stopAt < startAt)
            {
                switch (type)
                {
                    case SECOND:
                        max = 60;
                        break;
                    case MINUTE:
                        max = 60;
                        break;
                    case HOUR:
                        max = 24;
                        break;
                    case MONTH:
                        max = 12;
                        break;
                    case DAY_OF_WEEK:
                        max = 7;
                        break;
                    case DAY_OF_MONTH:
                        max = 31;
                        break;
                    case YEAR:
                        throw new ArgumentException("Start year must be less than stop year");
                    default:
                        throw new ArgumentException("Unexpected type encountered");
                }
                stopAt += max;
            }

            for (int i = startAt; i <= stopAt; i += incr)
            {
                if (max == -1)
                {
                    // ie: there's no max to overflow over
                    set.Add(Convert.ToInt32(i));
                }
                else
                {
                    // take the modulus to get the real value
                    int i2 = i % max;

                    // 1-indexed ranges should not include 0, and should include
                    // their max
                    if (i2 == 0 && (type == MONTH || type == DAY_OF_WEEK || type == DAY_OF_MONTH))
                    {
                        i2 = max;
                    }

                    set.Add(Convert.ToInt32(i2));
                }
            }
        }

        protected internal virtual SortedSet<int> GetSet(int type)
        {
            switch (type)
            {
                case SECOND:
                    return seconds;
                case MINUTE:
                    return minutes;
                case HOUR:
                    return hours;
                case DAY_OF_MONTH:
                    return daysOfMonth;
                case MONTH:
                    return months;
                case DAY_OF_WEEK:
                    return daysOfWeek;
                case YEAR:
                    return years;
                default:
                    return null;
            }
        }

        protected internal virtual ValueSet GetValue(int v, string s, int i)
        {
            char c = s[i];
            StringBuilder s1 = new StringBuilder(v.ToString());
            while (c >= '0' && c <= '9')
            {
                s1.Append(c);
                i++;
                if (i >= s.Length)
                {
                    break;
                }
                c = s[i];
            }
            ValueSet val = new ValueSet
            {
                pos = (i < s.Length) ? i : i + 1,
                value = int.Parse(s1.ToString())
            };
            return val;
        }

        protected internal virtual int GetNumericValue(string s, int i)
        {
            int endOfVal = FindNextWhiteSpace(i, s);
            string val = s.Substring(i, endOfVal - i);
            return int.Parse(val);
        }

        protected internal virtual int GetMonthNumber(string s)
        {
            int? integer = (int?)monthMap[s];

            if (integer is null)
            {
                return -1;
            }

            return integer.Value;
        }

        protected internal virtual int GetDayOfWeekNumber(string s)
        {
            int? integer = dayMap[s];

            if (integer is null)
            {
                return -1;
            }

            return integer.Value;
        }

        // //////////////////////////////////////////////////////////////////////////
        //
        // Computation Functions
        //
        // //////////////////////////////////////////////////////////////////////////

        //public virtual DateTime getTimeAfter(DateTime afterTime)
        //{
        //    GregorianCalendar date = new GregorianCalendar();
        //    date = new DateTime(afterTime);

        //    DateTime after = getTimeAfter(date);

        //    return after is null ? null : after;
        //}

        public virtual DateTime? GetTimeAfter(DateTime afterTime)
        {
            // Computation is based on Gregorian year only.
            DateTime cl = clockReader.GetCurrentCalendar(TimeZone); // new
                                                                    // java.util.GregorianCalendar(getTimeZone());

            // move ahead one second, since we're computing the time *after* the
            // given time
            afterTime.AddSeconds(1);
            // CronTrigger does not deal with milliseconds
            //cl.TimeInMillis = afterTime.Ticks;
            cl = cl.Set(DatePart.MILLISECOND, 0);

            bool gotOne = false;
            // loop until we've computed the next time, or we've past the endTime
            while (!gotOne)
            {

                // if (endTime is object && cl.getTime().after(endTime)) return null;
                if (cl.Year > 2999)
                { // prevent endless loop...
                    return null;
                }

                SortedSet<int> st = null;
                int t = 0;

                int sec = cl.Second;
                int min = cl.Minute;

                // get second.................................................
                st = new SortedSet<int>(seconds.Where(x => x >= sec));
                if (st is object && st.Count > 0)
                {
                    sec = ((int?)st.Min).Value;
                }
                else
                {
                    sec = ((int?)seconds.Min).Value;
                    min++;
                    cl = cl.Set(DatePart.MINUTE, min);
                }
                cl = cl.Set(DatePart.SECOND, sec);

                min = cl.Minute;
                int hr = cl.Hour;
                t = -1;

                // get minute.................................................
                st = new SortedSet<int>(minutes.Where(x => x >= min));
                if (st is object && st.Count > 0)
                {
                    t = min;
                    min = ((int?)st.Min).Value;
                }
                else
                {
                    min = ((int?)minutes.Min).Value;
                    hr++;
                }
                if (min != t)
                {
                    cl = cl.Set(DatePart.SECOND, 0).Set(DatePart.MINUTE, min);
                    cl = SetCalendarHour(cl, hr);
                    continue;
                }
                cl = cl.Set(DatePart.MINUTE, min);

                hr = cl.Hour;
                int day = cl.Day;
                t = -1;

                // get hour...................................................
                st = new SortedSet<int>(hours.Where(x => x >= hr));
                if (st is object && st.Count > 0)
                {
                    t = hr;
                    hr = ((int?)st.Min).Value;
                }
                else
                {
                    hr = ((int?)hours.Min).Value;
                    day++;
                }
                if (hr != t)
                {
                    cl = cl.Set(DatePart.SECOND, 0).Set(DatePart.MINUTE, 0).Set(DatePart.DAY, day);
                    cl = SetCalendarHour(cl, hr);
                    continue;
                }
                cl = cl.Set(DatePart.HOUR, hr);

                day = cl.Day;
                int mon = cl.Month + 1;
                // '+ 1' because calendar is 0-based for this field, and we are
                // 1-based
                t = -1;
                int tmon = mon;

                // get day...................................................
                bool dayOfMSpec = !daysOfMonth.Contains(NO_SPEC.Value);
                bool dayOfWSpec = !daysOfWeek.Contains(NO_SPEC.Value);
                if (dayOfMSpec && !dayOfWSpec)
                { // get day by day of month rule
                    st = new SortedSet<int>(daysOfMonth.Where(x => x >= day));
                    if (lastdayOfMonth)
                    {
                        if (!nearestWeekday)
                        {
                            t = day;
                            day = GetLastDayOfMonth(mon, cl.Year);
                            day -= lastdayOffset;
                        }
                        else
                        {
                            t = day;
                            day = GetLastDayOfMonth(mon, cl.Year);
                            day -= lastdayOffset;

                            // java.util.Calendar tcal =
                            // java.util.Calendar.getInstance(getTimeZone());
                            DateTime tcal = clockReader.GetCurrentCalendar(TimeZone);
                            tcal = new DateTime(cl.Year, mon - 1, day, 0, 0, 0);

                            int ldom = GetLastDayOfMonth(mon, cl.Year);
                            DayOfWeek dow = tcal.DayOfWeek;

                            if (dow == DayOfWeek.Saturday && day == 1)
                            {
                                day += 2;
                            }
                            else if (dow == DayOfWeek.Saturday)
                            {
                                day -= 1;
                            }
                            else if (dow == DayOfWeek.Sunday && day == ldom)
                            {
                                day -= 2;
                            }
                            else if (dow == DayOfWeek.Sunday)
                            {
                                day += 1;
                            }

                            tcal = new DateTime(tcal.Year, mon - 1, day, hr, min, sec);
                            // Date nTime = tcal.getTime();
                            if (tcal < afterTime)
                            {
                                day = 1;
                                mon++;
                            }
                        }
                    }
                    else if (nearestWeekday)
                    {
                        t = day;
                        day = ((int?)daysOfMonth.Min).Value;

                        // java.util.Calendar tcal =
                        // java.util.Calendar.getInstance(getTimeZone());
                        DateTime tcal = clockReader.GetCurrentCalendar(TimeZone);
                        tcal = new DateTime(cl.Year, mon - 1, day, 0, 0, 0);
                        int ldom = GetLastDayOfMonth(mon, cl.Year);
                        DayOfWeek dow = tcal.DayOfWeek;

                        if (dow == DayOfWeek.Saturday && day == 1)
                        {
                            day += 2;
                        }
                        else if (dow == DayOfWeek.Saturday)
                        {
                            day -= 1;
                        }
                        else if (dow == DayOfWeek.Sunday && day == ldom)
                        {
                            day -= 2;
                        }
                        else if (dow == DayOfWeek.Sunday)
                        {
                            day += 1;
                        }

                        tcal = new DateTime(tcal.Year, mon - 1, day, hr, min, sec);
                        // Date nTime = tcal.getTime();
                        if (tcal < afterTime)
                        {
                            day = ((int?)daysOfMonth.Min).Value;
                            mon++;
                        }
                    }
                    else if (st is object && st.Count > 0)
                    {
                        t = day;
                        day = ((int?)st.Min).Value;
                        // make sure we don't over-run a short month, such as
                        // february
                        int lastDay = GetLastDayOfMonth(mon, cl.Year);
                        if (day > lastDay)
                        {
                            day = ((int?)daysOfMonth.Min).Value;
                            mon++;
                        }
                    }
                    else
                    {
                        day = ((int?)daysOfMonth.Min).Value;
                        mon++;
                    }

                    if (day != t || mon != tmon)
                    {
                        cl = new DateTime(cl.Year, mon - 1, day, hr, min, sec);
                        // '- 1' because calendar is 0-based for this field, and we
                        // are 1-based
                        continue;
                    }
                }
                else if (dayOfWSpec && !dayOfMSpec)
                { // get day by day of week
                  // rule
                    if (lastdayOfWeek)
                    { // are we looking for the last XXX day of
                      // the month?
                        int dow = ((int?)daysOfWeek.Min).Value; // desired
                                                                // d-o-w
                        DayOfWeek cDow = cl.DayOfWeek; // current d-o-w
                        int daysToAdd = 0;
                        if ((int)cDow < (int)dow)
                        {
                            daysToAdd = (int)dow - (int)cDow;
                        }
                        if ((int)cDow > (int)dow)
                        {
                            daysToAdd = (int)dow + (7 - (int)cDow);
                        }

                        int lDay = GetLastDayOfMonth(mon, cl.Year);

                        if (day + daysToAdd > lDay)
                        { // did we already miss the
                          // last one?
                            cl = new DateTime(cl.Year, mon, 1, 0, 0, 0);
                            // no '- 1' here because we are promoting the month
                            continue;
                        }

                        // find date of last occurrence of this day in this month...
                        while ((day + daysToAdd + 7) <= lDay)
                        {
                            daysToAdd += 7;
                        }

                        day += daysToAdd;

                        if (daysToAdd > 0)
                        {
                            cl = new DateTime(cl.Year, mon - 1, day, 0, 0, 0);
                            // '- 1' here because we are not promoting the month
                            continue;
                        }

                    }
                    else if (nthdayOfWeek != 0)
                    {
                        // are we looking for the Nth XXX day in the month?
                        int dow = ((int?)daysOfWeek.Min).Value; // desired
                                                                // d-o-w
                        int cDow = (int)cl.DayOfWeek; // current d-o-w
                        int daysToAdd = 0;
                        if (cDow < dow)
                        {
                            daysToAdd = dow - cDow;
                        }
                        else if (cDow > dow)
                        {
                            daysToAdd = dow + (7 - cDow);
                        }

                        bool dayShifted = false;
                        if (daysToAdd > 0)
                        {
                            dayShifted = true;
                        }

                        day += daysToAdd;
                        int weekOfMonth = day / 7;
                        if (day % 7 > 0)
                        {
                            weekOfMonth++;
                        }

                        daysToAdd = (nthdayOfWeek - weekOfMonth) * 7;
                        day += daysToAdd;
                        if (daysToAdd < 0 || day > GetLastDayOfMonth(mon, cl.Year))
                        {
                            cl = new DateTime(cl.Year, mon, 1, 0, 0, 0);
                            // no '- 1' here because we are promoting the month
                            continue;
                        }
                        else if (daysToAdd > 0 || dayShifted)
                        {
                            cl = new DateTime(cl.Year, mon - 1, day, 0, 0, 0);
                            // '- 1' here because we are NOT promoting the month
                            continue;
                        }
                    }
                    else
                    {
                        int cDow = (int)cl.DayOfWeek; // current d-o-w
                        int dow = ((int?)daysOfWeek.Min).Value; // desired
                                                                // d-o-w
                        st = new SortedSet<int>(daysOfWeek.Where(x => x >= cDow));
                        if (st is object && st.Count > 0)
                        {
                            dow = ((int?)st.Min).Value;
                        }

                        int daysToAdd = 0;
                        if (cDow < dow)
                        {
                            daysToAdd = dow - cDow;
                        }
                        if (cDow > dow)
                        {
                            daysToAdd = dow + (7 - cDow);
                        }

                        int lDay = GetLastDayOfMonth(mon, cl.Year);

                        if (day + daysToAdd > lDay)
                        { // will we pass the end of
                          // the month?
                            cl = new DateTime(cl.Year, mon, 1, 0, 0, 0);
                            // no '- 1' here because we are promoting the month
                            continue;
                        }
                        else if (daysToAdd > 0)
                        { // are we switching days?
                            cl = new DateTime(cl.Year, mon - 1, day + daysToAdd, 0, 0, 0);
                            // '- 1' because calendar is 0-based for this field,
                            // and we are 1-based
                            continue;
                        }
                    }
                }
                else
                { // dayOfWSpec && !dayOfMSpec
                    throw new System.NotSupportedException("Support for specifying both a day-of-week AND a day-of-month parameter is not implemented.");
                    // TODO:
                }
                cl = cl.Set(DatePart.DAY, day);

                mon = cl.Month + 1;
                // '+ 1' because calendar is 0-based for this field, and we are
                // 1-based
                int year = cl.Year;
                t = -1;

                // test for expressions that never generate a valid fire date,
                // but keep looping...
                if (year > MAX_YEAR)
                {
                    return null;
                }

                // get month...................................................
                st = new SortedSet<int>(months.Where(x => x >= mon));
                if (st is object && st.Count > 0)
                {
                    t = mon;
                    mon = ((int?)st.Min).Value;
                }
                else
                {
                    mon = ((int?)months.Min).Value;
                    year++;
                }
                if (mon != t)
                {
                    cl = new DateTime(year, mon - 1, 1, 0, 0, 0);
                    // '- 1' because calendar is 0-based for this field, and we are
                    // 1-based
                    continue;
                }
                cl = cl.Set(DatePart.MONTH, mon - 1);
                // '- 1' because calendar is 0-based for this field, and we are
                // 1-based

                year = cl.Year;
                t = -1;

                // get year...................................................
                st = new SortedSet<int>(years.Where(x => x >= year));
                if (st is object && st.Count > 0)
                {
                    t = year;
                    year = ((int?)st.Min).Value;
                }
                else
                {
                    return null; // ran out of years...
                }

                if (year != t)
                {
                    cl = new DateTime(year, 1, 1, 0, 0, 0);
                    // '- 1' because calendar is 0-based for this field, and we are
                    // 1-based
                    continue;
                }
                cl = cl.Set(DatePart.YEAR, year);

                gotOne = true;
            } // while( !done )

            return cl;
        }

        /// <summary>
        /// Advance the calendar to the particular hour paying particular attention to daylight saving problems.
        /// </summary>
        /// <param name="cal"> </param>
        /// <param name="hour"> </param>
        protected internal virtual DateTime SetCalendarHour(DateTime cal, int hour)
        {
            cal = cal.Set(DatePart.HOUR, hour);
            if (cal.Hour != hour && hour != 24)
            {
                cal = cal.Set(DatePart.HOUR, hour + 1);
            }

            return cal;
        }

        protected internal virtual bool IsLeapYear(int year)
        {
            return ((year % 4 == 0 && year % 100 != 0) || (year % 400 == 0));
        }

        protected internal virtual int GetLastDayOfMonth(int monthNum, int year)
        {
            switch (monthNum)
            {
                case 1:
                    return 31;
                case 2:
                    return (IsLeapYear(year)) ? 29 : 28;
                case 3:
                    return 31;
                case 4:
                    return 30;
                case 5:
                    return 31;
                case 6:
                    return 30;
                case 7:
                    return 31;
                case 8:
                    return 31;
                case 9:
                    return 30;
                case 10:
                    return 31;
                case 11:
                    return 30;
                case 12:
                    return 31;
                default:
                    throw new System.ArgumentException("Illegal month number: " + monthNum);
            }
        }

        public object Clone()
        {
            return new CronExpression(cronExpression, clockReader, timeZone);
        }
    }

    public class ValueSet
    {

        public int value;

        public int pos;
    }

}