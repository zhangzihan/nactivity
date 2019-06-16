using System;

namespace Sys.Workflow.engine.impl.calendar
{
    using Sys.Workflow.engine.runtime;
    using System.Globalization;

    /// <summary>
    /// This class implements business calendar based on internal clock
    /// </summary>
    public abstract class BusinessCalendarImpl : IBusinessCalendar
    {

        protected internal IClockReader clockReader;

        public BusinessCalendarImpl(IClockReader clockReader)
        {
            this.clockReader = clockReader;
        }

        public virtual DateTime? ResolveDuedate(string duedateDescription)
        {
            return ResolveDuedate(duedateDescription, -1);
        }

        public abstract DateTime? ResolveDuedate(string duedateDescription, int maxIterations);

        public virtual bool? ValidateDuedate(string duedateDescription, int maxIterations, DateTime? endDate, DateTime? newTimer)
        {
            return !endDate.HasValue || endDate > newTimer || endDate.Equals(newTimer);
        }

        public virtual DateTime? ResolveEndDate(string endDateString)
        {
            if (DateTime.TryParse(endDateString, CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal, out var date))
            {
                return null;
            }

            return date;
        }
    }
}