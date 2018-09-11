using System;

namespace org.activiti.engine.impl.calendar
{

    using org.activiti.engine.runtime;

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
        DateTime? resolve(string duedateDescription, IClockReader clockReader, TimeZone timeZone);

    }

}