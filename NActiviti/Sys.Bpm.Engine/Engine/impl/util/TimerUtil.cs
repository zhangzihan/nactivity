using System;

namespace org.activiti.engine.impl.util
{
    using org.activiti.bpmn.model;
    using org.activiti.engine;
    using org.activiti.engine.@delegate;
    using org.activiti.engine.impl.calendar;
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.el;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.runtime;
    using System.Xml;

    /// 
    public class TimerUtil
    {
        public static object NoExecutionVariableScope { get; private set; }

        /// <summary>
        /// The event definition on which the timer is based.
        /// 
        /// Takes in an optional execution, if missing the <seealso cref="NoExecutionVariableScope"/> will be used (eg Timer start event)
        /// </summary>
        public static ITimerJobEntity CreateTimerEntityForTimerEventDefinition(TimerEventDefinition timerEventDefinition, bool isInterruptingTimer, IExecutionEntity executionEntity, string jobHandlerType, string jobHandlerConfig)
        {

            ProcessEngineConfigurationImpl processEngineConfiguration = Context.ProcessEngineConfiguration;

            string businessCalendarRef = null;
            IExpression expression = null;
            ExpressionManager expressionManager = processEngineConfiguration.ExpressionManager;

            // ACT-1415: timer-declaration on start-event may contain expressions NOT
            // evaluating variables but other context, evaluating should happen nevertheless
            IVariableScope scopeForExpression = executionEntity;
            if (scopeForExpression == null)
            {
                //scopeForExpression = NoExecutionVariableScope.SharedInstance;
            }

            if (!string.IsNullOrWhiteSpace(timerEventDefinition.TimeDate))
            {

                businessCalendarRef = DueDateBusinessCalendar.NAME;
                expression = expressionManager.CreateExpression(timerEventDefinition.TimeDate);

            }
            else if (!string.IsNullOrWhiteSpace(timerEventDefinition.TimeCycle))
            {

                businessCalendarRef = CycleBusinessCalendar.NAME;
                expression = expressionManager.CreateExpression(timerEventDefinition.TimeCycle);

            }
            else if (!string.IsNullOrWhiteSpace(timerEventDefinition.TimeDuration))
            {

                businessCalendarRef = DurationBusinessCalendar.NAME;
                expression = expressionManager.CreateExpression(timerEventDefinition.TimeDuration);
            }

            if (!string.IsNullOrWhiteSpace(timerEventDefinition.CalendarName))
            {
                businessCalendarRef = timerEventDefinition.CalendarName;
                IExpression businessCalendarExpression = expressionManager.CreateExpression(businessCalendarRef);
                businessCalendarRef = businessCalendarExpression.GetValue(scopeForExpression).ToString();
            }

            if (expression == null)
            {
                throw new ActivitiException("Timer needs configuration (either timeDate, timeCycle or timeDuration is needed) (" + timerEventDefinition.Id + ")");
            }

            IBusinessCalendar businessCalendar = processEngineConfiguration.BusinessCalendarManager.GetBusinessCalendar(businessCalendarRef);

            string dueDateString = null;
            DateTime? duedate = null;

            object dueDateValue = expression.GetValue(scopeForExpression);
            if (dueDateValue is string)
            {
                dueDateString = (string)dueDateValue;
            }
            else if (dueDateValue is DateTime)
            {
                duedate = (DateTime)dueDateValue;
            }
            else if (dueDateValue is Nullable<DateTime>)
            {
                //JodaTime support
                duedate = (DateTime?)dueDateValue;
            }
            else if (dueDateValue != null)
            {
                throw new ActivitiException("Timer '" + executionEntity.ActivityId + "' was not configured with a valid duration/time, either hand in a java.util.Date or a String in format 'yyyy-MM-dd'T'hh:mm:ss'");
            }

            if (duedate == null && !string.IsNullOrWhiteSpace(dueDateString))
            {
                duedate = businessCalendar.ResolveDuedate(dueDateString);
            }

            ITimerJobEntity timer = null;
            if (duedate != null)
            {
                timer = Context.CommandContext.TimerJobEntityManager.Create();
                timer.JobType = JobFields.JOB_TYPE_TIMER;
                timer.Revision = 1;
                timer.JobHandlerType = jobHandlerType;
                timer.JobHandlerConfiguration = jobHandlerConfig;
                timer.Exclusive = true;
                timer.Retries = processEngineConfiguration.AsyncExecutorNumberOfRetries;
                timer.Duedate = duedate;
                if (executionEntity != null)
                {
                    timer.Execution = executionEntity;
                    timer.ProcessDefinitionId = executionEntity.ProcessDefinitionId;
                    timer.ProcessInstanceId = executionEntity.ProcessInstanceId;

                    // Inherit tenant identifier (if applicable)
                    if (!string.IsNullOrWhiteSpace(executionEntity.TenantId))
                    {
                        timer.TenantId = executionEntity.TenantId;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(timerEventDefinition.TimeCycle))
            {
                // See ACT-1427: A boundary timer with a cancelActivity='true', doesn't need to repeat itself
                bool repeat = !isInterruptingTimer;

                // ACT-1951: intermediate catching timer events shouldn't repeat according to spec
                if (executionEntity != null)
                {
                    FlowElement currentElement = executionEntity.CurrentFlowElement;
                    if (currentElement is IntermediateCatchEvent)
                    {
                        repeat = false;
                    }
                }

                if (repeat)
                {
                    string prepared = PrepareRepeat(dueDateString);
                    timer.Repeat = prepared;
                }
            }

            if (timer != null && executionEntity != null)
            {
                timer.Execution = executionEntity;
                timer.ProcessDefinitionId = executionEntity.ProcessDefinitionId;

                // Inherit tenant identifier (if applicable)
                if (!string.IsNullOrWhiteSpace(executionEntity.TenantId))
                {
                    timer.TenantId = executionEntity.TenantId;
                }
            }

            return timer;
        }

        public static string PrepareRepeat(string dueDate)
        {
            if (dueDate.StartsWith("R", StringComparison.Ordinal) && dueDate.Split("/", true).Length == 2)
            {
                //如果当前只设置了一个时间，那么就将当前时间做为循环的开始时间。
                //保证循环格式 R0/开始时间/结束时间
                return dueDate.Replace("/", $"/{DateTime.Now.ToString("o")}/");
            }
            return dueDate;
        }

        /// <summary>
        /// 默认日期ToString
        /// </summary>
        /// <param name="date">日期</param>
        /// <param name="format">yyyy-MM-dd HH:mm:ss</param>
        /// <returns></returns>
        public static string ToString(DateTime date)
        {
            return ToString(date, null);
        }

        /// <summary>
        /// 默认日期ToString
        /// </summary>
        /// <param name="date">日期</param>
        /// <param name="format">yyyy-MM-dd HH:mm:ss</param>
        /// <returns></returns>
        public static string ToString(DateTime date, string format)
        {
            return date.ToString(string.IsNullOrWhiteSpace(format) ? "yyyy-MM-dd HH:mm:ss" : format);
        }

        /// <summary>
        /// 默认日期To Period string format PYMDTHMS
        /// </summary>
        /// <param name="date">日期</param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string ToPeriod(DateTime date)
        {
            return XmlConvert.ToString(date, null);
        }

        /// <summary>
        /// 默认日期To Period string format PYMDTHMS
        /// </summary>
        /// <param name="date">日期</param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string ToPeriod(DateTime date, string format)
        {
            return XmlConvert.ToString(date, format);
        }
    }

}