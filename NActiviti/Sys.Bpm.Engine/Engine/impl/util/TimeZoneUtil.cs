using System;

namespace Sys.Workflow.engine.impl.util
{

    public class TimeZoneUtil
    {

        public static DateTime ConvertToTimeZone(DateTime time, TimeZoneInfo timeZone)
        {
            return TimeZoneInfo.ConvertTime(time, timeZone ?? TimeZoneInfo.Local);
            //DateTime foreignTime = new GregorianCalendar(timeZone);
            //foreignTime.TimeInMillis = time.Ticks;

            //return foreignTime;
            //return DateTime.Now;
        }

    }

}