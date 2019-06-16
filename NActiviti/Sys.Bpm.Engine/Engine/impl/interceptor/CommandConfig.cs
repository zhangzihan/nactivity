namespace Sys.Workflow.Engine.Impl.Interceptor
{
    using Sys.Workflow.Engine.Impl.Cfg;

    /// <summary>
    /// Configuration settings for the command interceptor chain.
    /// 
    /// Instances of this class are immutable, and thus thread- and share-safe.
    /// 
    /// 
    /// </summary>
    public class CommandConfig
    {

        private bool contextReusePossible;
        private TransactionPropagation propagation;

        public CommandConfig()
        {
            this.contextReusePossible = false;
            this.propagation = TransactionPropagation.REQUIRED;
        }

        public CommandConfig(bool contextReusePossible)
        {
            this.contextReusePossible = contextReusePossible;
            this.propagation = TransactionPropagation.REQUIRED;
        }

        public CommandConfig(bool contextReusePossible, TransactionPropagation transactionPropagation)
        {
            this.contextReusePossible = contextReusePossible;
            this.propagation = transactionPropagation;
        }

        protected internal CommandConfig(CommandConfig commandConfig)
        {
            this.contextReusePossible = commandConfig.contextReusePossible;
            this.propagation = commandConfig.propagation;
        }

        public virtual bool ContextReusePossible
        {
            get
            {
                return contextReusePossible;
            }
        }

        public virtual TransactionPropagation TransactionPropagation
        {
            get
            {
                return propagation;
            }
        }

        public virtual CommandConfig SetContextReusePossible(bool contextReusePossible)
        {
            CommandConfig config = new CommandConfig(this)
            {
                contextReusePossible = contextReusePossible
            };
            return config;
        }

        public virtual CommandConfig TransactionRequired()
        {
            CommandConfig config = new CommandConfig(this)
            {
                propagation = TransactionPropagation.REQUIRED
            };
            return config;
        }

        public virtual CommandConfig TransactionRequiresNew()
        {
            CommandConfig config = new CommandConfig
            {
                contextReusePossible = false,
                propagation = TransactionPropagation.REQUIRES_NEW
            };
            return config;
        }

        public virtual CommandConfig TransactionNotSupported()
        {
            CommandConfig config = new CommandConfig
            {
                contextReusePossible = false,
                propagation = TransactionPropagation.NOT_SUPPORTED
            };
            return config;
        }
    }

}