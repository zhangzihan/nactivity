namespace Sys.Workflow.Engine.Impl.Persistence
{
    using MassTransit;
    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using System;

    /// <summary>
    /// <seealso cref="IIdGenerator"/> implementation based on the current time and the ethernet address of the machine it is running on.
    /// 
    /// 
    /// </summary>
    public class StrongUuidGenerator : IIdGenerator
    {
        // different ProcessEngines on the same classloader share one generator.
        protected internal static Guid? timeBasedGenerator;
        private readonly object syncRoot = new object();

        public StrongUuidGenerator()
        {
            EnsureGeneratorInitialized();
        }

        protected internal virtual void EnsureGeneratorInitialized()
        {
            if (timeBasedGenerator is null)
            {
                lock (syncRoot)
                {
                    if (timeBasedGenerator is null)
                    {
                        timeBasedGenerator = NewId.NextGuid();
                    }
                }
            }
        }

        public virtual string GetNextId()
        {
            return timeBasedGenerator.ToString();
        }

    }

}