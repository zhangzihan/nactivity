namespace org.activiti.engine.impl.cfg
{
    using org.activiti.engine.impl.interceptor;

    /// <summary>
    /// Command executor that passes commands to the first interceptor in the chain. If no <seealso cref="CommandConfig"/> is passed, the default configuration will be used.
    /// 
    /// 
    /// 
    /// </summary>
    public class CommandExecutorImpl : ICommandExecutor
    {

        protected internal CommandConfig defaultConfig;
        protected internal ICommandInterceptor first;

        public CommandExecutorImpl(CommandConfig defaultConfig, ICommandInterceptor first)
        {
            this.defaultConfig = defaultConfig;
            this.first = first;
        }

        public virtual ICommandInterceptor First
        {
            get
            {
                return first;
            }
            set
            {
                this.first = value;
            }
        }


        public virtual CommandConfig DefaultConfig
        {
            get
            {
                return defaultConfig;
            }
        }

        public virtual T execute<T>(ICommand<T> command)
        {
            return execute(defaultConfig, command);
        }

        public virtual T execute<T>(CommandConfig config, ICommand<T> command)
        {
            return first.execute(config, command);
        }

    }

}