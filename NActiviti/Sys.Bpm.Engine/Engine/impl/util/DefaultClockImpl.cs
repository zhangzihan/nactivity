using System;

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
namespace org.activiti.engine.impl.util
{

    /// 
    public class DefaultClockImpl : org.activiti.engine.runtime.IClock
    {

        private static DateTime? CURRENT_TIME;

        public virtual DateTime CurrentTime
        {
            private set
            {
                DateTime? time = null;

                if (value != null)
                {
                    time = new DateTime(value.Ticks);
                }

                CurrentCalendar = time.GetValueOrDefault();
            }
            get
            {
                CURRENT_TIME = DateTime.Now;// CURRENT_TIME.HasValue ? CURRENT_TIME : DateTime.Now;

                return CURRENT_TIME.Value;
            }
        }

        public virtual DateTime CurrentCalendar
        {
            set
            {
                CURRENT_TIME = value;
            }
            get
            {
                return new DateTime(CURRENT_TIME.GetValueOrDefault().Ticks);
            }
        }

        public virtual void Reset()
        {
            CURRENT_TIME = DateTime.Now;
        }



        public virtual DateTime GetCurrentCalendar(TimeZoneInfo timeZone)
        {
            return TimeZoneUtil.ConvertToTimeZone(CurrentCalendar, timeZone);
        }

        public virtual TimeZoneInfo CurrentTimeZone
        {
            get
            {
                return TimeZoneInfo.Local;
            }
        }

    }

}