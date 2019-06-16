using System;
using System.Collections.Generic;

namespace Sys.Workflow.engine.impl.variable
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Bson;
    using Sys.Workflow.engine.impl.context;
    using Sys.Workflow.engine.impl.interceptor;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;

    /// <summary>
    /// Variable type capable of storing a list of reference to JPA-entities. Only JPA-Entities which are configured by annotations are supported. Use of compound primary keys is not supported. <br>
    /// The variable value should be of type <seealso cref="List"/> and can only contain objects of the same type.
    /// 
    /// 
    /// </summary>
    public class JPAEntityListVariableType : AbstractVariableType, ICacheableVariable, IJPAEntityListVariableType
    {

        public const string TYPE_NAME = "jpa-entity-list";

        protected internal JPAEntityMappings mappings;

        protected internal bool forceCachedValue;

        public JPAEntityListVariableType()
        {
            mappings = new JPAEntityMappings();
        }

        public virtual bool ForceCacheable
        {
            set
            {
                this.forceCachedValue = value;
            }
        }

        public override string TypeName
        {
            get
            {
                return TYPE_NAME;
            }
        }

        public override bool Cachable
        {
            get
            {
                return forceCachedValue;
            }
        }

        public override bool IsAbleToStore(object value)
        {
            bool canStore = false;

            if (value is IList<object> list)
            {
                if (list.Count > 0)
                {
                    // We can only store the list if we are sure it's actually a
                    // list of JPA entities. In case the
                    // list is empty, we don't store it.
                    canStore = true;
                    Type entityClass = mappings.GetEntityMetaData(list[0].GetType()).EntityClass;

                    foreach (object entity in list)
                    {
                        canStore = entity != null && mappings.IsJPAEntity(entity) && mappings.GetEntityMetaData(entity.GetType()).EntityClass.Equals(entityClass);
                        if (!canStore)
                        {
                            // In case the object is not a JPA entity or the class
                            // doesn't match, we can't store the list
                            break;
                        }
                    }
                }
            }
            return canStore;
        }

        public override void SetValue(object value, IValueFields valueFields)
        {
            IEntityManagerSession entityManagerSession = Context.CommandContext.GetSession<IEntityManagerSession>();
            if (entityManagerSession == null)
            {
                throw new ActivitiException("Cannot set JPA variable: " + typeof(IEntityManagerSession) + " not configured");
            }
            else
            {
                // Before we set the value we must flush all pending changes from
                // the entitymanager
                // If we don't do this, in some cases the primary key will not yet
                // be set in the object
                // which will cause exceptions down the road.
                entityManagerSession.Flush();
            }

            if (value is IList<object> && ((IList<object>)value).Count > 0)
            {
                IList<object> list = (IList<object>)value;
                IList<string> ids = new List<string>();

                string type = mappings.GetJPAClassString(list[0]);
                foreach (object entry in list)
                {
                    ids.Add(mappings.GetJPAIdString(entry));
                }

                // Store type in text field and the ID's as a serialized array
                valueFields.Bytes = SerializeIds(ids);
                valueFields.TextValue = type;

            }
            else if (value == null)
            {
                valueFields.Bytes = null;
                valueFields.TextValue = null;
            }
            else
            {
                throw new ActivitiIllegalArgumentException("Value is not a list of JPA entities: " + value);
            }

        }

        public override object GetValue(IValueFields valueFields)
        {
            byte[] bytes = valueFields.Bytes;
            if (!(valueFields.TextValue is null) && !(bytes is null))
            {
                string entityClass = valueFields.TextValue;

                IList<object> result = new List<object>();
                string[] ids = DeserializeIds(bytes);

                foreach (string id in ids)
                {
                    result.Add(mappings.GetJPAEntity(entityClass, id));
                }

                return result;
            }
            return null;
        }

        /// <returns> a bytearray containing all ID's in the given string serialized as an array. </returns>
        protected internal virtual byte[] SerializeIds(IList<string> ids)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (BsonDataWriter datawriter = new BsonDataWriter(ms))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Serialize(datawriter, ids.ToArray());
                        return ms.ToArray();
                    }
                }
            }
            catch (IOException ioe)
            {
                throw new ActivitiException("Unexpected exception when serializing JPA id's", ioe);
            }
        }

        protected internal virtual string[] DeserializeIds(byte[] bytes)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    using (BsonDataReader reader = new BsonDataReader(ms))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        return serializer.Deserialize<string[]>(reader);
                    }
                }
            }
            catch (IOException ioe)
            {
                throw new ActivitiException("Unexpected exception when deserializing JPA id's", ioe);
            }
            catch (Exception e)
            {
                throw new ActivitiException("Unexpected exception when deserializing JPA id's", e);
            }
        }
    }
}