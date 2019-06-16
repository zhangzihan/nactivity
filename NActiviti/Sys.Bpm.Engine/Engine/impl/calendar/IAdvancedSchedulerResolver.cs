using System;

namespace Sys.Workflow.Engine.Impl.Calendars
{

    using Sys.Workflow.Engine.Runtime;

    /// <summary>
    /// Provides an interface for versioned due date resolvers.
    /// 
    /// 
    /// </summary>
    public interface IAdvancedSchedulerResolver
    {

        /// <summary>
        /// Resolves a due date using the specified time zone (if supported)
        /// </summary>
        /// <param name="duedateDescription">
        ///          An original Activiti schedule string in either ISO or CRON format </param>
        /// <param name="clockReader">
        ///          The time provider </param>
        /// <param name="timeZone">
        ///          The time zone to use in the calculations </param>
        /// <returns> The due date </returns>
        DateTime? Resolve(string duedateDescription, IClockReader clockReader, TimeZoneInfo timeZone);

    }

}