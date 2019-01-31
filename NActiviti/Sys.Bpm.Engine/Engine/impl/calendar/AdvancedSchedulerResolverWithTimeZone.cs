using System;

namespace org.activiti.engine.impl.calendar
{

    using org.activiti.engine.runtime;

    /// <summary>
    /// Resolves a due date taking into account the specified time zone.
    /// 
    /// 
    /// </summary>
    public class AdvancedSchedulerResolverWithTimeZone : IAdvancedSchedulerResolver
    {

        public virtual DateTime? resolve(string duedateDescription, IClockReader clockReader, TimeZoneInfo timeZone)
        {
            DateTime? nextRun = null;

            try
            {
                if (duedateDescription.StartsWith("R", StringComparison.Ordinal))
                {
                    nextRun = (new DurationHelper(duedateDescription, clockReader)).getCalendarAfter(clockReader.getCurrentCalendar(timeZone));
                }
                else
                {
                    nextRun = (new CronExpression(duedateDescription, clockReader, timeZone)).getTimeAfter(clockReader.getCurrentCalendar(timeZone));
                }

            }
            catch (Exception e)
            {
                throw new ActivitiException("Failed to parse scheduler expression: " + duedateDescription, e);
            }

            return nextRun == null ? null : nextRun;
        }

    }

}