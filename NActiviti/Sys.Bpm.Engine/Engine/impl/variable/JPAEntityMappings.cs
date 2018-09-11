using System;
using System.Collections.Generic;

/* Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace org.activiti.engine.impl.variable
{
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.util;
    using Sys.Data;

    /// 
    public class JPAEntityMappings
    {

        private IDictionary<string, EntityMetaData> classMetaDatamap;

        private JPAEntityScanner enitityScanner;

        public JPAEntityMappings()
        {
            classMetaDatamap = new Dictionary<string, EntityMetaData>();
            enitityScanner = new JPAEntityScanner();
        }

        public virtual bool isJPAEntity(object value)
        {
            if (value != null)
            {
                // EntityMetaData will be added for all classes, even those who are
                // not
                // JPA-entities to prevent unneeded annotation scanning
                return getEntityMetaData(value.GetType()).JPAEntity;
            }
            return false;
        }

        public virtual EntityMetaData getEntityMetaData(Type clazz)
        {
            EntityMetaData metaData = classMetaDatamap[clazz.FullName];
            if (metaData == null)
            {
                // Class not present in meta-data map, create metaData for it and
                // add
                metaData = scanClass(clazz);
                classMetaDatamap[clazz.FullName] = metaData;
            }
            return metaData;
        }

        private EntityMetaData scanClass(Type clazz)
        {
            return enitityScanner.scanClass(clazz);
        }

        public virtual string getJPAClassString(object value)
        {
            if (value == null)
            {
                throw new ActivitiIllegalArgumentException("null value cannot be saved");
            }

            EntityMetaData metaData = getEntityMetaData(value.GetType());
            if (!metaData.JPAEntity)
            {
                throw new ActivitiIllegalArgumentException("Object is not a JPA Entity: class='" + value.GetType() + "', " + value);
            }

            // Extract the class from the Entity instance
            return metaData.EntityClass.FullName;
        }

        public virtual string getJPAIdString(object value)
        {
            EntityMetaData metaData = getEntityMetaData(value.GetType());
            if (!metaData.JPAEntity)
            {
                throw new ActivitiIllegalArgumentException("Object is not a JPA Entity: class='" + value.GetType() + "', " + value);
            }
            object idValue = getIdValue(value, metaData);
            return getIdString(idValue);
        }

        public virtual object getIdValue(object value, EntityMetaData metaData)
        {
            try
            {
                if (metaData.IdMethod != null)
                {
                    return metaData.IdMethod.Invoke(value, null);
                }
                else if (metaData.IdField != null)
                {
                    return metaData.IdField.GetValue(value);
                }
            }
            catch (System.ArgumentException iae)
            {
                throw new ActivitiException("Illegal argument exception when getting value from id method/field on JPAEntity", iae);
            }
            //catch (Exception iae)
            //{
            //  throw new ActivitiException("Cannot access id method/field for JPA Entity", iae);
            //}
            catch (Exception ite)
            {
                throw new ActivitiException("Exception occurred while getting value from id field/method on JPAEntity: " + ite.InnerException.Message, ite.InnerException);
            }

            // Fall trough when no method and field is set
            throw new ActivitiException("Cannot get id from JPA Entity, no id method/field set");
        }

        public virtual object getJPAEntity(string className, string idString)
        {
            Type entityClass = null;
            entityClass = ReflectUtil.loadClass(className);

            EntityMetaData metaData = getEntityMetaData(entityClass);

            // Create primary key of right type
            object primaryKey = createId(metaData, idString);
            return findEntity(entityClass, primaryKey);
        }

        private object findEntity(Type entityClass, object primaryKey)
        {
            IEntityManager em = Context.CommandContext.getSession<IEntityManagerSession>().EntityManager;

            object entity = em.find(entityClass, primaryKey);
            if (entity == null)
            {
                throw new ActivitiException("Entity does not exist: " + entityClass.FullName + " - " + primaryKey);
            }
            return entity;
        }

        public virtual object createId(EntityMetaData metaData, string @string)
        {
            Type type = metaData.IdType;
            // According to JPA-spec all primitive types (and wrappers) are
            // supported, String, util.Date, sql.Date,
            // BigDecimal and BigInteger
            if (type == typeof(long) || type == typeof(long))
            {
                return long.Parse(@string);
            }
            else if (type == typeof(string))
            {
                return @string;
            }
            else if (type == typeof(Byte) || type == typeof(sbyte))
            {
                return sbyte.Parse(@string);
            }
            else if (type == typeof(short) || type == typeof(short))
            {
                return short.Parse(@string);
            }
            else if (type == typeof(int) || type == typeof(int))
            {
                return int.Parse(@string);
            }
            else if (type == typeof(float) || type == typeof(float))
            {
                return float.Parse(@string);
            }
            else if (type == typeof(Double) || type == typeof(double))
            {
                return double.Parse(@string);
            }
            else if (type == typeof(char) || type == typeof(char))
            {
                return new char?(@string[0]);
            }
            else if (type == typeof(DateTime))
            {
                return new DateTime(long.Parse(@string));
            }
            else if (type == typeof(DateTime))
            {
                return new DateTime(long.Parse(@string));
            }
            else if (type == typeof(decimal))
            {
                return decimal.Parse(@string);
            }
            else if (type == typeof(System.Numerics.BigInteger))
            {
                return Int64.Parse(@string);
            }
            else if (type == typeof(System.Guid))
            {
                return System.Guid.Parse(@string);
            }
            else
            {
                throw new ActivitiIllegalArgumentException("Unsupported Primary key type for JPA-Entity: " + type.FullName);
            }
        }

        public virtual string getIdString(object value)
        {
            if (value == null)
            {
                throw new ActivitiIllegalArgumentException("Value of primary key for JPA-Entity cannot be null");
            }
            // Only java.sql.date and java.util.date require custom handling, the
            // other types
            // can just use toString()
            if (value is DateTime)
            {
                return "" + ((DateTime)value).Ticks;
            }
            else if (value is DateTime)
            {
                return "" + ((DateTime)value).Ticks;
            }
            else if (value is long? || value is string || value is sbyte? || value is short? || value is int? || value is float? || value is double? || value is char? || value is decimal || value is System.Numerics.BigInteger || value is System.Guid)
            {
                return value.ToString();
            }
            else
            {
                throw new ActivitiIllegalArgumentException("Unsupported Primary key type for JPA-Entity: " + value.GetType().FullName);
            }
        }
    }
}