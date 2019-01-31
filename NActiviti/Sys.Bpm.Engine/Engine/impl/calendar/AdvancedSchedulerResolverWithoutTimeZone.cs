using System;

namespace org.activiti.engine.impl.calendar
{

    using org.activiti.engine.runtime;

    /// <summary>
    /// Resolves a due date using the original Activiti due date resolver. This does not take into account the passed time zone.
    /// 
    /// 
    /// </summary>
    public class AdvancedSchedulerResolverWithoutTimeZone : IAdvancedSchedulerResolver
    {

        public virtual DateTime? resolve(string duedateDescription, IClockReader clockReader, TimeZoneInfo timeZone)
        {
            return (new CycleBusinessCalendar(clockReader)).resolveDuedate(duedateDescription);
        }

    }

}