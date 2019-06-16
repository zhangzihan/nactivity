using System;

namespace Sys.Workflow.Engine.Impl.Persistence.Entity
{
    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow;
    using System.Collections.Generic;

    /// <summary>
    /// <para>
    /// Encapsulates the logic for transparently working with <seealso cref="IByteArrayEntity"/> .
    /// </para>
    /// 
    /// 
    /// </summary>
    [Serializable]
    public class ByteArrayRef : IByteArrayRef
    {
        private const long serialVersionUID = 1L;

        private string id;
        private string name;
        private IByteArrayEntity entity;
        protected internal bool deleted;

        public ByteArrayRef()
        {
        }

        // Only intended to be used by ByteArrayRefTypeHandler
        public ByteArrayRef(string id)
        {
            this.id = id;
        }

        public virtual string Id
        {
            get
            {
                return id;
            }
        }

        public virtual string Name
        {
            get
            {
                return name;
            }
        }

        public virtual byte[] Bytes
        {
            get
            {
                EnsureInitialized();
                return (entity?.Bytes);
            }
            set
            {
                if (id is null)
                {
                    if (value != null)
                    {
                        IByteArrayEntityManager byteArrayEntityManager = Context.CommandContext.ByteArrayEntityManager;
                        entity = byteArrayEntityManager.Create();
                        entity.Name = name;
                        entity.Bytes = value;
                        byteArrayEntityManager.Insert(entity);
                        id = entity.Id;
                    }
                }
                else
                {
                    EnsureInitialized();
                    entity.Bytes = value;
                }
            }
        }

        public virtual void SetValue(string name, byte[] bytes)
        {
            this.name = name;
            Bytes = bytes;
        }


        public virtual IByteArrayEntity Entity
        {
            get
            {
                EnsureInitialized();
                return entity;
            }
        }

        public virtual void Delete()
        {
            if (!deleted && !(id is null))
            {
                if (entity != null)
                {
                    // if the entity has been loaded already,
                    // we might as well use the safer optimistic locking delete.
                    Context.CommandContext.ByteArrayEntityManager.Delete(entity);
                }
                else
                {
                    Context.CommandContext.ByteArrayEntityManager.DeleteByteArrayById(id);
                }
                entity = null;
                id = null;
                deleted = true;
            }
        }

        public void EnsureInitialized()
        {
            var ctx = Context.CommandContext;
            if (id != null && entity == null)
            {
                if (ctx == null)
                {
                    var pi = ProcessEngineServiceProvider.Resolve<IProcessEngine>().ProcessEngineConfiguration as ProcessEngineConfigurationImpl;

                    if (pi == null)
                    {
                        return;
                    }

                    ctx = pi.CommandContextFactory.CreateCommandContext(new ByteArrayRefCmd(id));
                    Context.CommandContext = ctx;
                }

                entity = ctx.ByteArrayEntityManager.FindById<IByteArrayEntity>(new KeyValuePair<string, object>("id", id));
                name = entity.Name;
            }
        }

        public virtual bool Deleted
        {
            get
            {
                return deleted;
            }
        }

        public override string ToString()
        {
            return "ByteArrayRef[id=" + id + ", name=" + name + ", entity=" + entity + (deleted ? ", deleted]" : "]");
        }

        private class ByteArrayRefCmd : ICommand<IByteArrayEntity>
        {
            private readonly string id;

            public ByteArrayRefCmd(string id)
            {
                this.id = id;
            }


            public IByteArrayEntity Execute(ICommandContext commandContext)
            {
                return commandContext.ByteArrayEntityManager.FindById<IByteArrayEntity>(new KeyValuePair<string, object>("id", id));
            }
        }
    }

}