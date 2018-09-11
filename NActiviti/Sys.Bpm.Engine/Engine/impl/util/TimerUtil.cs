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

    /// 
    public class TimerUtil
    {
        public static object NoExecutionVariableScope { get; private set; }

        /// <summary>
        /// The event definition on which the timer is based.
        /// 
        /// Takes in an optional execution, if missing the <seealso cref="NoExecutionVariableScope"/> will be used (eg Timer start event)
        /// </summary>
        public static ITimerJobEntity createTimerEntityForTimerEventDefinition(TimerEventDefinition timerEventDefinition, bool isInterruptingTimer, IExecutionEntity executionEntity, string jobHandlerType, string jobHandlerConfig)
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
                expression = expressionManager.createExpression(timerEventDefinition.TimeDate);

            }
            else if (!string.IsNullOrWhiteSpace(timerEventDefinition.TimeCycle))
            {

                businessCalendarRef = CycleBusinessCalendar.NAME;
                expression = expressionManager.createExpression(timerEventDefinition.TimeCycle);

            }
            else if (!string.IsNullOrWhiteSpace(timerEventDefinition.TimeDuration))
            {

                businessCalendarRef = DurationBusinessCalendar.NAME;
                expression = expressionManager.createExpression(timerEventDefinition.TimeDuration);
            }

            if (!string.IsNullOrWhiteSpace(timerEventDefinition.CalendarName))
            {
                businessCalendarRef = timerEventDefinition.CalendarName;
                IExpression businessCalendarExpression = expressionManager.createExpression(businessCalendarRef);
                businessCalendarRef = businessCalendarExpression.getValue(scopeForExpression).ToString();
            }

            if (expression == null)
            {
                throw new ActivitiException("Timer needs configuration (either timeDate, timeCycle or timeDuration is needed) (" + timerEventDefinition.Id + ")");
            }

            IBusinessCalendar businessCalendar = processEngineConfiguration.BusinessCalendarManager.getBusinessCalendar(businessCalendarRef);

            string dueDateString = null;
            DateTime? duedate = null;

            object dueDateValue = expression.getValue(scopeForExpression);
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
                duedate = businessCalendar.resolveDuedate(dueDateString);
            }

            ITimerJobEntity timer = null;
            if (duedate != null)
            {
                timer = Context.CommandContext.TimerJobEntityManager.create();
                timer.JobType = Job_Fields.JOB_TYPE_TIMER;
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
                    string prepared = prepareRepeat(dueDateString);
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

        public static string prepareRepeat(string dueDate)
        {
            if (dueDate.StartsWith("R", StringComparison.Ordinal) && dueDate.Split("/", true).Length == 2)
            {
                //SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss");
                //return dueDate.Replace("/", "/" + sdf.format(Context.ProcessEngineConfiguration.Clock.CurrentTime) + "/");
                return DateTime.Parse(dueDate).ToString("yyyy-MM-dd HH:mm:ss");
            }
            return dueDate;
        }

    }

}