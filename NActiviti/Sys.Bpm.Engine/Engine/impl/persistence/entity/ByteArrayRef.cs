using System;

namespace org.activiti.engine.impl.persistence.entity
{

    using org.activiti.engine.impl.context;
    using System.Collections.Generic;

    /// <summary>
    /// <para>
    /// Encapsulates the logic for transparently working with <seealso cref="IByteArrayEntity"/> .
    /// </para>
    /// 
    /// 
    /// </summary>
    [Serializable]
    public class ByteArrayRef
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
                ensureInitialized();
                return (entity != null ? entity.Bytes : null);
            }
            set
            {
                if (ReferenceEquals(id, null))
                {
                    if (value != null)
                    {
                        IByteArrayEntityManager byteArrayEntityManager = Context.CommandContext.ByteArrayEntityManager;
                        entity = byteArrayEntityManager.create();
                        entity.Name = name;
                        entity.Bytes = value;
                        byteArrayEntityManager.insert(entity);
                        id = entity.Id;
                    }
                }
                else
                {
                    ensureInitialized();
                    entity.Bytes = value;
                }
            }
        }

        public virtual void setValue(string name, byte[] bytes)
        {
            this.name = name;
            Bytes = bytes;
        }


        public virtual IByteArrayEntity Entity
        {
            get
            {
                ensureInitialized();
                return entity;
            }
        }

        public virtual void delete()
        {
            if (!deleted && !ReferenceEquals(id, null))
            {
                if (entity != null)
                {
                    // if the entity has been loaded already,
                    // we might as well use the safer optimistic locking delete.
                    Context.CommandContext.ByteArrayEntityManager.delete(entity);
                }
                else
                {
                    Context.CommandContext.ByteArrayEntityManager.deleteByteArrayById(id);
                }
                entity = null;
                id = null;
                deleted = true;
            }
        }

        public void ensureInitialized()
        {
            var ctx = Context.CommandContext;
            if (id != null && entity == null && ctx != null)
            {
                entity = ctx.ByteArrayEntityManager.findById<IByteArrayEntity>(new KeyValuePair<string, object>("id", id));
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
    }

}