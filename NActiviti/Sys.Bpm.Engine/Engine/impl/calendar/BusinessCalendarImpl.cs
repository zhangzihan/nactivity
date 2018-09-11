using System;

namespace org.activiti.engine.impl.calendar
{
    using org.activiti.engine.runtime;
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

        public virtual DateTime? resolveDuedate(string duedateDescription)
        {
            return resolveDuedate(duedateDescription, -1);
        }

        public abstract DateTime? resolveDuedate(string duedateDescription, int maxIterations);

        public virtual bool? validateDuedate(string duedateDescription, int maxIterations, DateTime? endDate, DateTime? newTimer)
        {
            return !endDate.HasValue || endDate > newTimer || endDate.Equals(newTimer);
        }

        public virtual DateTime? resolveEndDate(string endDateString)
        {
            if (DateTime.TryParse(endDateString, CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal, out var date))
            {
                return null;
            }

            return date;
        }
    }
}