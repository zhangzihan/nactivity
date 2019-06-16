using System;

namespace Sys.Workflow.engine.impl.calendar
{

    using Sys.Workflow.engine.runtime;

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