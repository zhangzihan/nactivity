namespace org.activiti.engine.impl.cmd
{
    using org.activiti.engine.impl.interceptor;

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