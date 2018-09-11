using System;

namespace org.activiti.engine.impl.util
{

    public class TimeZoneUtil
    {

        public static DateTime convertToTimeZone(DateTime time, TimeZone timeZone)
        {
            //DateTime foreignTime = new GregorianCalendar(timeZone);
            //foreignTime.TimeInMillis = time.Ticks;

            //return foreignTime;
            return DateTime.Now;
        }

    }

}