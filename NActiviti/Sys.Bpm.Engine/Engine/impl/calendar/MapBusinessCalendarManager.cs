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
namespace Sys.Workflow.engine.impl.calendar
{

    /// 
    public class MapBusinessCalendarManager : IBusinessCalendarManager
    {

        private readonly IDictionary<string, IBusinessCalendar> businessCalendars;

        public MapBusinessCalendarManager()
        {
            this.businessCalendars = new Dictionary<string, IBusinessCalendar>();
        }

        public MapBusinessCalendarManager(IDictionary<string, IBusinessCalendar> businessCalendars)
        {
            if (businessCalendars == null)
            {
                throw new System.ArgumentException("businessCalendars can not be null");
            }

            this.businessCalendars = new Dictionary<string, IBusinessCalendar>(businessCalendars);
        }

        public virtual IBusinessCalendar GetBusinessCalendar(string businessCalendarRef)
        {
            IBusinessCalendar businessCalendar = businessCalendars[businessCalendarRef];
            if (businessCalendar == null)
            {
                throw new ActivitiException("Requested business calendar " + businessCalendarRef + " does not exist. Allowed calendars are " + this.businessCalendars.Keys + ".");
            }
            return businessCalendar;
        }

        public virtual IBusinessCalendarManager AddBusinessCalendar(string businessCalendarRef, IBusinessCalendar businessCalendar)
        {
            businessCalendars[businessCalendarRef] = businessCalendar;
            return this;
        }
    }
}