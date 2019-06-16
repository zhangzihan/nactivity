using System;
using System.Collections.Generic;
using System.Linq;

/*
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace Sys.Workflow.engine.impl.calendar
{
    using Sys.Workflow.engine.impl.util;
    using Sys.Workflow.engine.runtime;
    using System.Globalization;
    using System.Xml;

    /// <summary>
    /// helper class for parsing ISO8601 duration format (also recurring) and computing next timer date
    /// </summary>
    public class DurationHelper
    {
        private readonly DateTime? start;
        private readonly DateTime? end;
        private readonly TimeSpan period;
        private readonly bool isRepeat;
        private readonly int times;
        private readonly int maxIterations = -1;
        private readonly bool repeatWithNoBounds;

        public virtual DateTime? Start
        {
            get
            {
                return start;
            }
        }

        public virtual DateTime? End
        {
            get
            {
                return end;
            }
        }

        public virtual TimeSpan Period
        {
            get
            {
                return period;
            }
        }

        public virtual bool Repeat
        {
            get
            {
                return isRepeat;
            }
        }

        public virtual int Times
        {
            get
            {
                return times;
            }
        }

        protected internal IClockReader clockReader;

        public DurationHelper(string expressionS, int maxIterations, IClockReader clockReader)
        {
            this.clockReader = clockReader;
            this.maxIterations = maxIterations;
            IList<string> expression = new List<string>(expressionS.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries));
            //datatypeFactory = DatatypeFactory.newInstance();

            if (expression.Count > 3 || expression.Count == 0)
            {
                throw new ActivitiIllegalArgumentException("Cannot parse duration");
            }
            if (expression[0].StartsWith("R", StringComparison.Ordinal))
            {
                isRepeat = true;
                times = expression[0].Length == 1 ? int.MaxValue - 1 : int.Parse(expression[0].Substring(1));

                if (expression[0].Equals("R"))
                { // R without params
                    repeatWithNoBounds = true;
                }

                expression = expression.Skip(1).Take(expression.Count).ToList();
            }

            if (IsDuration(expression[0]))
            {
                period = ParsePeriod(expression[0]);
                end = expression.Count == 1 ? null : (DateTime?)ParseDate(expression[1]);
            }
            else
            {
                start = ParseDate(expression[0]);
                if (IsDuration(expression[1]))
                {
                    period = ParsePeriod(expression[1]);
                }
                else
                {
                    end = ParseDate(expression[1]);
                    period = end.Value.Subtract(start.Value);// datatypeFactory.newDuration(end.getTimeInMillis() - start.getTimeInMillis());
                }
            }

            //if (isDuration(expression[0]))
            //{
            //    period = XmlConvert.ToTimeSpan(expression[0]);
            //    end = expression.Count == 1 ? null : (DateTime?)parseDate(expression[1]);
            //}
            //else
            //{
            //    start = expression.Count == 0 ? DateTime.Now : parseDate(expression[0]);
            //    if (isDuration(expression[0]))
            //    {
            //        period = XmlConvert.ToTimeSpan(expression[0]);
            //    }
            //    else if (expression.Count > 1)
            //    {
            //        end = parseDate(expression[1]);
            //        period = end.Value.Subtract(start.Value);
            //    }
            //    else
            //    {
            //        end = parseDate(expression[0]);
            //        period = end.Value.Subtract(start.Value);
            //    }
            //}
            if (start == null)
            {
                start = clockReader.CurrentCalendar;
            }
        }

        public DurationHelper(string expressionS, IClockReader clockReader) : this(expressionS, -1, clockReader)
        {
        }

        public virtual DateTime CalendarAfter
        {
            get
            {
                return GetCalendarAfter(clockReader.CurrentCalendar);
            }
        }

        public virtual DateTime GetCalendarAfter(DateTime time)
        {
            if (isRepeat)
            {
                return GetDateAfterRepeat(time);
            }
            // TODO: is this correct?
            if (end != null)
            {
                return end.Value;
            }
            return Add(start, period);
        }

        public virtual bool? IsValidDate(DateTime? newTimer)
        {
            return end == null || end > newTimer || end.Equals(newTimer);
        }

        public virtual DateTime? DateAfter
        {
            get
            {
                DateTime? date = CalendarAfter;

                return !date.HasValue ? null : date;
            }
        }

        private DateTime GetDateAfterRepeat(DateTime date)
        {
            DateTime current = TimeZoneUtil.ConvertToTimeZone(start.Value, TimeZoneInfo.Local);

            if (repeatWithNoBounds)
            {
                while (current < date || current.Equals(date))
                { // As long as current date is not past the engine date, we keep looping
                    DateTime newTime = Add(current, period);
                    if (newTime.Equals(current) || newTime < current)
                    {
                        break;
                    }
                    current = newTime;
                }
            }
            else
            {

                int maxLoops = times;
                if (maxIterations > 0)
                {
                    maxLoops = maxIterations - times;
                }
                for (int i = 0; i < maxLoops + 1 && current > date; i++)
                {
                    current = Add(current, period);
                }
            }
            return current < date ? date : TimeZoneUtil.ConvertToTimeZone(current, clockReader.CurrentTimeZone);
        }

        protected internal virtual DateTime Add(DateTime? date, TimeSpan duration)
        {
            DateTime calendar = new DateTime(date.Value.Ticks);

            // duration.addTo does not account for daylight saving time (xerces),
            // reversing order of addition fixes the problem
            calendar.AddSeconds(duration.Seconds);
            calendar.AddMinutes(duration.Minutes);
            calendar.AddHours(duration.Hours);
            calendar.AddDays(duration.Days);
            calendar.AddDays(duration.TotalDays);
            calendar.AddYears((int)duration.TotalDays);

            return calendar;
        }

        protected internal virtual DateTime ParseDate(string date)
        {
            DateTime? dateCalendar;
            try
            {
                dateCalendar = DateTime.Parse(date).ToLocalTime();//ISODateTimeFormat.dateTimeParser().withZone(DateTimeZone.forTimeZone(clockReader.CurrentTimeZone)).parseDateTime(date).toCalendar(null);
            }
            catch (System.ArgumentException)
            {
                // try to parse a java.util.date to string back to a java.util.date
                //dateCalendar = new GregorianCalendar();
                //DateFormat DATE_FORMAT = new SimpleDateFormat("EEE MMM dd kk:mm:ss z yyyy", Locale.ENGLISH);
                //dateCalendar = new DateTime(DATE_FORMAT.parse(date));
                dateCalendar = DateTime.Parse(date, new CultureInfo("en-us")
                {
                    DateTimeFormat = new DateTimeFormatInfo()
                    {
                        LongTimePattern = "EEE MMM dd kk:mm:ss z yyyy"
                    }
                });
            }

            return dateCalendar.Value;
        }

        protected internal virtual TimeSpan ParsePeriod(string period)
        {
            return XmlConvert.ToTimeSpan(period);
        }

        protected internal virtual bool IsDuration(string time)
        {
            return time.StartsWith("P", StringComparison.Ordinal);
        }
    }
}