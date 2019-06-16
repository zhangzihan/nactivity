namespace Sys.Workflow.Engine
{
    using Sys.Workflow.Engine.Runtime;

    /// <summary>
    /// This exception is thrown when you try to execute a job that is not found (may be due to cancelActiviti="true" for instance)..
    /// 
    /// 
    /// </summary>
    public class JobNotFoundException : ActivitiObjectNotFoundException
    {

        private const long serialVersionUID = 1L;

        /// <summary>
        /// the id of the job </summary>
        private string jobId;

        public JobNotFoundException(string jobId) : base("No job found with id '" + jobId + "'.", typeof(IJob))
        {
            this.jobId = jobId;
        }

        public virtual string JobId
        {
            get
            {
                return this.jobId;
            }
        }

    }

}