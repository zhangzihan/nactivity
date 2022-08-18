using System;
using Sys.Workflow.Engine.Impl.Cfg;
using Sys.Workflow.Engine.Impl.Contexts;
using Sys.Workflow.Engine.Impl.Interceptor;
using Sys.Workflow;
using System.Collections.Generic;

namespace Sys.Workflow.Engine.Impl.Persistence.Entity
{
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
        /// <summary>
        /// 
        /// </summary>
        protected internal bool deleted;
        /// <summary>
        /// 
        /// </summary>
        public ByteArrayRef()
        {
        }

        /// <summary>
        /// Only intended to be used by ByteArrayRefTypeHandler
        /// </summary>
        /// <param name="id"></param>
        public ByteArrayRef(string id)
        {
            this.id = id;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string Id
        {
            get
            {
                return id;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual string Name
        {
            get
            {
                return name;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual byte[] Bytes
        {
            get
            {
                EnsureInitialized();
                return entity?.Bytes;
            }
            set
            {
                if (id is null)
                {
                    if (value is not null)
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="bytes"></param>
        public virtual void SetValue(string name, byte[] bytes)
        {
            this.name = name;
            Bytes = bytes;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IByteArrayEntity Entity
        {
            get
            {
                EnsureInitialized();
                return entity;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual void Delete()
        {
            if (!deleted && id is not null)
            {
                if (entity is object)
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
        /// <summary>
        /// 
        /// </summary>
        public void EnsureInitialized()
        {
            var ctx = Context.CommandContext;
            if (id is not null && entity is null)
            {
                if (ctx is null)
                {
                    if (ProcessEngineServiceProvider.Resolve<IProcessEngine>().ProcessEngineConfiguration is not ProcessEngineConfigurationImpl pi)
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
        /// <summary>
        /// 
        /// </summary>
        public virtual bool Deleted
        {
            get
            {
                return deleted;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
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