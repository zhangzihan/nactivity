using System;
using System.Collections.Generic;

/* Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace org.activiti.engine.impl.calendar
{

    using org.activiti.engine.impl.context;
    using System.Globalization;

    /// 
    public class DefaultBusinessCalendar : IBusinessCalendar
    {
        private static readonly IDictionary<string, int> units = new Dictionary<string, int>();

        static DefaultBusinessCalendar()
        {
            units["millis"] = (int)DatePart.MILLISECOND;
            units["seconds"] = (int)DatePart.SECOND;
            units["second"] = (int)DatePart.SECOND;
            units["minute"] = (int)DatePart.MINUTE;
            units["minutes"] = (int)DatePart.MINUTE;
            units["hour"] = (int)DatePart.HOUR;
            units["hours"] = (int)DatePart.HOUR;
            units["day"] = (int)DatePart.DAY_OF_YEAR;
            units["days"] = (int)DatePart.DAY_OF_YEAR;
            units["week"] = (int)DatePart.WEEK_OF_YEAR;
            units["weeks"] = (int)DatePart.WEEK_OF_YEAR;
            units["month"] = (int)DatePart.MONTH;
            units["months"] = (int)DatePart.MONTH;
            units["year"] = (int)DatePart.YEAR;
            units["years"] = (int)DatePart.YEAR;
        }

        public virtual DateTime? ResolveDuedate(string duedateDescription, int maxIterations)
        {
            return ResolveDuedate(duedateDescription);
        }

        public virtual DateTime? ResolveDuedate(string duedate)
        {
            DateTime resolvedDuedate = Context.ProcessEngineConfiguration.Clock.CurrentTime;

            string[] tokens = duedate.Split(" and ", true);
            foreach (string token in tokens)
            {
                resolvedDuedate = AddSingleUnitQuantity(resolvedDuedate, token);
            }

            return resolvedDuedate;
        }

        public virtual bool? ValidateDuedate(string duedateDescription, int maxIterations, DateTime? endDate, DateTime? newTimer)
        {
            return true;
        }

        public virtual DateTime? ResolveEndDate(string endDate)
        {
            return null;
        }

        protected internal virtual DateTime AddSingleUnitQuantity(DateTime startDate, string singleUnitQuantity)
        {
            int spaceIndex = singleUnitQuantity.IndexOf(" ", StringComparison.Ordinal);
            if (spaceIndex == -1 || singleUnitQuantity.Length < spaceIndex + 1)
            {
                throw new ActivitiIllegalArgumentException("invalid duedate format: " + singleUnitQuantity);
            }

            string quantityText = singleUnitQuantity.Substring(0, spaceIndex);
            int quantity = Convert.ToInt32(quantityText);

            string unitText = singleUnitQuantity.Substring(spaceIndex + 1).Trim().ToLower();

            int unit = units[unitText];

            startDate = startDate.Add((DatePart)unit, quantity);

            return startDate;
        }
    }

}