namespace Sys.Workflow.engine
{
    /// <summary>
    /// This exception is thrown when you try to claim a task that is already claimed by someone else.
    /// 
    /// 
    /// 
    /// </summary>
    public class ActivitiTaskAlreadyClaimedException : ActivitiException
    {

        private const long serialVersionUID = 1L;

        /// <summary>
        /// the id of the task that is already claimed </summary>
        private string taskId;

        /// <summary>
        /// the assignee of the task that is already claimed </summary>
        private string taskAssignee;

        public ActivitiTaskAlreadyClaimedException(string taskId, string taskAssignee) : base("Task '" + taskId + "' is already claimed by someone else.")
        {
            this.taskId = taskId;
            this.taskAssignee = taskAssignee;
        }

        public virtual string TaskId
        {
            get
            {
                return this.taskId;
            }
        }

        public virtual string TaskAssignee
        {
            get
            {
                return this.taskAssignee;
            }
        }

    }

}