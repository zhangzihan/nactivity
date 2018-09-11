namespace org.activiti.engine.impl.persistence
{
    using org.activiti.engine.impl.cfg;
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
            ensureGeneratorInitialized();
        }

        protected internal virtual void ensureGeneratorInitialized()
        {
            if (timeBasedGenerator == null)
            {
                lock (typeof(StrongUuidGenerator))
                {
                    if (timeBasedGenerator == null)
                    {
                        timeBasedGenerator = Guid.NewGuid();
                    }
                }
            }
        }

        public virtual string NextId
        {
            get
            {
                return timeBasedGenerator.ToString();
            }
        }

    }

}