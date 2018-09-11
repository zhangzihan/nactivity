namespace org.activiti.engine.impl.interceptor
{
    using org.activiti.engine.impl.cfg;

    /// <summary>
    /// A listener that can be used to be notified of lifecycle events of the <seealso cref="CommandContext"/>.
    /// 
    /// 
    /// </summary>
    public interface ICommandContextCloseListener
    {

        /// <summary>
        /// Called when the <seealso cref="CommandContext"/> is being closed, but no 'close logic' has been executed.
        /// 
        /// At this point, the <seealso cref="ITransactionContext"/> (if applicable) has not yet been committed/rolledback 
        /// and none of the <seealso cref="ISession"/> instances have been flushed.
        /// 
        /// If an exception happens and it is not caught in this method:
        /// - The <seealso cref="ISession"/> instances will *not* be flushed
        /// - The <seealso cref="ITransactionContext"/> will be rolled back (if applicable) 
        /// </summary>
        void closing(ICommandContext commandContext);

        /// <summary>
        /// Called when the <seealso cref="ISession"/> have been successfully flushed.
        /// When an exception happened during the flushing of the sessions, this method will not be called.
        /// 
        /// If an exception happens and it is not caught in this method:
        /// - The <seealso cref="ISession"/> instances will *not* be flushed
        /// - The <seealso cref="ITransactionContext"/> will be rolled back (if applicable) 
        /// </summary>
        void afterSessionsFlush(ICommandContext commandContext);

        /// <summary>
        /// Called when the <seealso cref="CommandContext"/> is successfully closed.
        /// 
        /// At this point, the <seealso cref="ITransactionContext"/> (if applicable) has been successfully committed
        /// and no rollback has happened. All <seealso cref="ISession"/> instances have been closed.
        /// 
        /// Note that throwing an exception here does *not* affect the transaction. 
        /// The <seealso cref="CommandContext"/> will log the exception though.
        /// </summary>
        void closed(ICommandContext commandContext);

        /// <summary>
        /// Called when the <seealso cref="CommandContext"/> has not been successully closed due to an exception that happened.
        /// 
        /// Note that throwing an exception here does *not* affect the transaction. 
        /// The <seealso cref="CommandContext"/> will log the exception though.
        /// </summary>
        void closeFailure(ICommandContext commandContext);

    }

}