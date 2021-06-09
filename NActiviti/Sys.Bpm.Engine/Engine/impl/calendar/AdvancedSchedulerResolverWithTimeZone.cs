using System;

namespace Sys.Workflow.Engine.Impl.Calendars
{

    using Sys.Workflow.Engine.Runtime;

    /// <summary>
    /// Resolves a due date taking into account the specified time zone.
    /// 
    /// 
    /// </summary>
    public class AdvancedSchedulerResolverWithTimeZone : IAdvancedSchedulerResolver
    {

        public virtual DateTime? Resolve(string duedateDescription, IClockReader clockReader, TimeZoneInfo timeZone)
        {
            DateTime? nextRun = null;

            try
            {
                if (duedateDescription.StartsWith("R", StringComparison.Ordinal))
                {
                    nextRun = (new DurationHelper(duedateDescription, clockReader)).GetCalendarAfter(clockReader.GetCurrentCalendar(timeZone));
                }
                else
                {
                    nextRun = (new CronExpression(duedateDescription, clockReader, timeZone)).GetTimeAfter(clockReader.GetCurrentCalendar(timeZone));
                }

            }
            catch (Exception e)
            {
                throw new ActivitiException("Failed to parse scheduler expression: " + duedateDescription, e);
            }

            return nextRun is null ? null : nextRun;
        }

    }

}