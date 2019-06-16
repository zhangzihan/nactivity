namespace Sys.Workflow.Engine.Impl.Cmd
{
    using Sys.Workflow.Engine.Impl.Interceptor;

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