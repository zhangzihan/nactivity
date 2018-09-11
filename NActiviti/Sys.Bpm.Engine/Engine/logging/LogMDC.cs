namespace org.activiti.engine.logging
{
    using org.activiti.engine.impl.persistence.entity;

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

        internal static bool enabled;

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


        public static void putMDCExecution(IExecutionEntity e)
        {
            //if (!string.ReferenceEquals(e.Id, null))
            //{
            //    MDC.put(LOG_MDC_EXECUTION_ID, e.Id);
            //}
            //if (!string.ReferenceEquals(e.ProcessDefinitionId, null))
            //{
            //    MDC.put(LOG_MDC_PROCESSDEFINITION_ID, e.ProcessDefinitionId);
            //}
            //if (!string.ReferenceEquals(e.ProcessInstanceId, null))
            //{
            //    MDC.put(LOG_MDC_PROCESSINSTANCE_ID, e.ProcessInstanceId);
            //}
            //if (!string.ReferenceEquals(e.ProcessInstanceBusinessKey, null))
            //{
            //    MDC.put(LOG_MDC_BUSINESS_KEY, e.ProcessInstanceBusinessKey);
            //}

        }

        public static void clear()
        {
            //MDC.clear();
        }
    }

}