using Newtonsoft.Json.Linq;
using System;

namespace Sys.Workflow.Engine.Impl.JobExecutors
{
    public class TimerEventHandler
    {

        public const string PROPERTYNAME_TIMER_ACTIVITY_ID = "activityId";
        public const string PROPERTYNAME_END_DATE_EXPRESSION = "timerEndDate";
        public const string PROPERTYNAME_CALENDAR_NAME_EXPRESSION = "calendarName";

        public static string CreateConfiguration(string id, string endDate, string calendarName)
        {
            JToken cfgJson = new JObject
            {
                [PROPERTYNAME_TIMER_ACTIVITY_ID] = id
            };
            if (!string.IsNullOrWhiteSpace(endDate))
            {
                cfgJson[PROPERTYNAME_END_DATE_EXPRESSION] = endDate;
            }
            if (!string.IsNullOrWhiteSpace(calendarName))
            {
                cfgJson[PROPERTYNAME_CALENDAR_NAME_EXPRESSION] = calendarName;
            }
            return cfgJson.ToString();
        }

        public static string SetActivityIdToConfiguration(string jobHandlerConfiguration, string activityId)
        {
            try
            {
                JToken cfgJson = JToken.FromObject(jobHandlerConfiguration);
                cfgJson[PROPERTYNAME_TIMER_ACTIVITY_ID] = activityId;
                return cfgJson.ToString();
            }
            catch (Exception)
            {
                return jobHandlerConfiguration;
            }
        }

        public static string GetActivityIdFromConfiguration(string jobHandlerConfiguration)
        {
            try
            {
                JToken cfgJson = JToken.FromObject(jobHandlerConfiguration);

                return cfgJson[PROPERTYNAME_TIMER_ACTIVITY_ID].ToString();
            }
            catch (Exception)
            {
                return jobHandlerConfiguration;
            }
        }

        public static string GeCalendarNameFromConfiguration(string jobHandlerConfiguration)
        {
            try
            {
                JToken cfgJson = JToken.FromObject(jobHandlerConfiguration);

                return cfgJson[PROPERTYNAME_CALENDAR_NAME_EXPRESSION].ToString();
            }
            catch (Exception)
            {
                // calendar name is not specified
                return "";
            }
        }

        public static string SetEndDateToConfiguration(string jobHandlerConfiguration, string endDate)
        {
            JToken cfgJson;
            try
            {
                cfgJson = JToken.FromObject(jobHandlerConfiguration);
            }
            catch (Exception)
            {
                // create the json config
                cfgJson = new JObject
                {
                    [PROPERTYNAME_TIMER_ACTIVITY_ID] = jobHandlerConfiguration
                };
            }
            if (!string.IsNullOrWhiteSpace(endDate))
            {
                cfgJson[PROPERTYNAME_END_DATE_EXPRESSION] = endDate;
            }

            return cfgJson.ToString();
        }

        public static string GetEndDateFromConfiguration(string jobHandlerConfiguration)
        {
            try
            {
                JToken cfgJson = JToken.FromObject(jobHandlerConfiguration);
                return cfgJson[PROPERTYNAME_END_DATE_EXPRESSION].ToString();
            }
            catch (Exception)
            {
                return null;
            }
        }

    }

}