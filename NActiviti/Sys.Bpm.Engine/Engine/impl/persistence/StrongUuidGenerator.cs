namespace org.activiti.engine.impl.persistence
{
    using MassTransit;
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.persistence.entity;
    using System;

    /// <summary>
    /// <seealso cref="IIdGenerator"/> implementation based on the current time and the ethernet address of the machine it is running on.
    /// 
    /// 
    /// </summary>
    public class StrongUuidGenerator : IIdGenerator
    {

        // different ProcessEngines on the same classloader share one generator.
        protected internal static Guid timeBasedGenerator;

        public StrongUuidGenerator()
        {
            EnsureGeneratorInitialized();
        }

        protected internal virtual void EnsureGeneratorInitialized()
        {
            if (timeBasedGenerator == null)
            {
                lock (typeof(StrongUuidGenerator))
                {
                    if (timeBasedGenerator == null)
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