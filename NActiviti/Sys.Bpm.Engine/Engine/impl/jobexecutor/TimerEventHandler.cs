using Newtonsoft.Json.Linq;
using System;

namespace org.activiti.engine.impl.jobexecutor
{
    public class TimerEventHandler
    {

        public const string PROPERTYNAME_TIMER_ACTIVITY_ID = "activityId";
        public const string PROPERTYNAME_END_DATE_EXPRESSION = "timerEndDate";
        public const string PROPERTYNAME_CALENDAR_NAME_EXPRESSION = "calendarName";

        public static string createConfiguration(string id, string endDate, string calendarName)
        {
            JToken cfgJson = new JObject();
            cfgJson[PROPERTYNAME_TIMER_ACTIVITY_ID] = id;
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

        public static string setActivityIdToConfiguration(string jobHandlerConfiguration, string activityId)
        {
            try
            {
                JToken cfgJson = JToken.FromObject(jobHandlerConfiguration);
                cfgJson[PROPERTYNAME_TIMER_ACTIVITY_ID] = activityId;
                return cfgJson.ToString();
            }
            catch (Exception ex)
            {
                return jobHandlerConfiguration;
            }
        }

        public static string getActivityIdFromConfiguration(string jobHandlerConfiguration)
        {
            try
            {
                JToken cfgJson = JToken.FromObject(jobHandlerConfiguration);

                return cfgJson[PROPERTYNAME_TIMER_ACTIVITY_ID].ToString();
            }
            catch (Exception ex)
            {
                return jobHandlerConfiguration;
            }
        }

        public static string geCalendarNameFromConfiguration(string jobHandlerConfiguration)
        {
            try
            {
                JToken cfgJson = JToken.FromObject(jobHandlerConfiguration);

                return cfgJson[PROPERTYNAME_CALENDAR_NAME_EXPRESSION].ToString();
            }
            catch (Exception ex)
            {
                // calendar name is not specified
                return "";
            }
        }

        public static string setEndDateToConfiguration(string jobHandlerConfiguration, string endDate)
        {
            JToken cfgJson = null;
            try
            {
                cfgJson = JToken.FromObject(jobHandlerConfiguration);
            }
            catch (Exception ex)
            {
                // create the json config
                cfgJson = new JObject();
                cfgJson[PROPERTYNAME_TIMER_ACTIVITY_ID] = jobHandlerConfiguration;
            }
            if (!string.IsNullOrWhiteSpace(endDate))
            {
                cfgJson[PROPERTYNAME_END_DATE_EXPRESSION] = endDate;
            }

            return cfgJson.ToString();
        }

        public static string getEndDateFromConfiguration(string jobHandlerConfiguration)
        {
            try
            {
                JToken cfgJson = JToken.FromObject(jobHandlerConfiguration);
                return cfgJson[PROPERTYNAME_END_DATE_EXPRESSION].ToString();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

    }

}