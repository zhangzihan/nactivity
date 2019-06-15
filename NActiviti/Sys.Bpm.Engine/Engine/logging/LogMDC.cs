namespace org.activiti.engine.logging
{
    using org.activiti.engine.impl.persistence.entity;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Constants and functions for MDC (Mapped Diagnostic Context) logging
    /// 
    /// 
    /// </summary>

    public class LogMDC
    {
        public const string LOG_MDC_PROCESSDEFINITION_ID = "mdcProcessDefinitionID";
        public const string LOG_MDC_EXECUTION_ID = "mdcExecutionId";
        public const string LOG_MDC_PROCESSINSTANCE_ID = "mdcProcessInstanceID";
        public const string LOG_MDC_BUSINESS_KEY = "mdcBusinessKey";
        public const string LOG_MDC_TASK_ID = "mdcTaskId";

        internal static bool enabled = true;

        private static ThreadLocal<IDictionary<string, object>> logger = new ThreadLocal<IDictionary<string, object>>();

        public static bool MDCEnabled
        {
            get
            {
                return enabled;
            }
            set
            {
                enabled = value;
            }
        }


        public static void PutMDCExecution(IExecutionEntity e)
        {
            IDictionary<string, object> mdc;
            if (logger.IsValueCreated)
            {
                logger.Value = new Dictionary<string, object>();
            }

            mdc = logger.Value;
            if (!(e.Id is null))
            {
                mdc.Add(LOG_MDC_EXECUTION_ID, e.Id);
            }
            if (!(e.ProcessDefinitionId is null))
            {
                mdc.Add(LOG_MDC_PROCESSDEFINITION_ID, e.ProcessDefinitionId);
            }
            if (!(e.ProcessInstanceId is null))
            {
                mdc.Add(LOG_MDC_PROCESSINSTANCE_ID, e.ProcessInstanceId);
            }
            if (!(e.ProcessInstanceBusinessKey is null))
            {
                mdc.Add(LOG_MDC_BUSINESS_KEY, e.ProcessInstanceBusinessKey);
            }
        }

        public static void Clear()
        {
            logger.Value = null;
        }

        public override string ToString()
        {
            if (logger.IsValueCreated)
            {
                var mdc = logger.Value;
                if (!(mdc is null))
                {
                    mdc.TryGetValue(LOG_MDC_PROCESSDEFINITION_ID, out var procid);
                    mdc.TryGetValue(LOG_MDC_EXECUTION_ID, out var execid);
                    mdc.TryGetValue(LOG_MDC_PROCESSINSTANCE_ID, out var pinstid);
                    mdc.TryGetValue(LOG_MDC_BUSINESS_KEY, out var bizid);
                    mdc.TryGetValue(LOG_MDC_TASK_ID, out var taskid);

                    return $"{LOG_MDC_PROCESSDEFINITION_ID}:{procid}{LOG_MDC_EXECUTION_ID}:{execid}{LOG_MDC_PROCESSINSTANCE_ID}:{pinstid}{LOG_MDC_BUSINESS_KEY}:{bizid}{LOG_MDC_TASK_ID}:{taskid}";
                }

                return "";
            }

            return "";
        }
    }

}