using System;

namespace Sys.Workflow.Engine.Impl.Calendars
{

    using Sys.Workflow.Engine.Runtime;

    /// <summary>
    /// Resolves a due date using the original Activiti due date resolver. This does not take into account the passed time zone.
    /// 
    /// 
    /// </summary>
    public class AdvancedSchedulerResolverWithoutTimeZone : IAdvancedSchedulerResolver
    {

        public virtual DateTime? Resolve(string duedateDescription, IClockReader clockReader, TimeZoneInfo timeZone)
        {
            return (new CycleBusinessCalendar(clockReader)).ResolveDuedate(duedateDescription);
        }

    }

}