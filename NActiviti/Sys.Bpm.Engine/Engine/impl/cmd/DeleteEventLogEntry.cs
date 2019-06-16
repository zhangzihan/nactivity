namespace Sys.Workflow.engine.impl.cmd
{
    using Sys.Workflow.engine.impl.interceptor;

    /// 
    public class DeleteEventLogEntry : ICommand<object>
    {

        protected internal long logNr;

        public DeleteEventLogEntry(long logNr)
        {
            this.logNr = logNr;
        }

        public virtual object Execute(ICommandContext commandContext)
        {
            commandContext.EventLogEntryEntityManager.DeleteEventLogEntry(logNr);

            return null;
        }

    }

}