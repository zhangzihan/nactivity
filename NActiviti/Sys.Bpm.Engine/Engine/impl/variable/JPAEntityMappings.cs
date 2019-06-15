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
    using System.Numerics;

    /// 
    public class JPAEntityMappings
    {

        private readonly IDictionary<string, EntityMetaData> classMetaDatamap;

        private readonly JPAEntityScanner enitityScanner;

        public JPAEntityMappings()
        {
            classMetaDatamap = new Dictionary<string, EntityMetaData>(StringComparer.OrdinalIgnoreCase);
            enitityScanner = new JPAEntityScanner();
        }

        public virtual bool IsJPAEntity(object value)
        {
            if (value != null)
            {
                // EntityMetaData will be added for all classes, even those who are
                // not
                // JPA-entities to prevent unneeded annotation scanning
                return GetEntityMetaData(value.GetType()).JPAEntity;
            }
            return false;
        }

        public virtual EntityMetaData GetEntityMetaData(Type clazz)
        {
            EntityMetaData metaData = classMetaDatamap[clazz.FullName];
            if (metaData == null)
            {
                // Class not present in meta-data map, create metaData for it and
                // add
                metaData = ScanClass(clazz);
                classMetaDatamap[clazz.FullName] = metaData;
            }
            return metaData;
        }

        private EntityMetaData ScanClass(Type clazz)
        {
            return enitityScanner.ScanClass(clazz);
        }

        public virtual string GetJPAClassString(object value)
        {
            if (value == null)
            {
                throw new ActivitiIllegalArgumentException("null value cannot be saved");
            }

            EntityMetaData metaData = GetEntityMetaData(value.GetType());
            if (!metaData.JPAEntity)
            {
                throw new ActivitiIllegalArgumentException("Object is not a JPA Entity: class='" + value.GetType() + "', " + value);
            }

            // Extract the class from the Entity instance
            return metaData.EntityClass.FullName;
        }

        public virtual string GetJPAIdString(object value)
        {
            EntityMetaData metaData = GetEntityMetaData(value.GetType());
            if (!metaData.JPAEntity)
            {
                throw new ActivitiIllegalArgumentException("Object is not a JPA Entity: class='" + value.GetType() + "', " + value);
            }
            object idValue = GetIdValue(value, metaData);
            return GetIdString(idValue);
        }

        public virtual object GetIdValue(object value, EntityMetaData metaData)
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
            catch (ArgumentException iae)
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

        public virtual object GetJPAEntity(string className, string idString)
        {
            Type entityClass = ReflectUtil.LoadClass(className);

            EntityMetaData metaData = GetEntityMetaData(entityClass);

            // Create primary key of right type
            object primaryKey = CreateId(metaData, idString);
            return FindEntity(entityClass, primaryKey);
        }

        private object FindEntity(Type entityClass, object primaryKey)
        {
            IEntityManager em = Context.CommandContext.GetSession<IEntityManagerSession>().EntityManager;

            object entity = em.Find(entityClass, primaryKey);
            if (entity is null)
            {
                throw new ActivitiException("Entity does not exist: " + entityClass.FullName + " - " + primaryKey);
            }
            return entity;
        }

        public virtual object CreateId(EntityMetaData metaData, string @string)
        {
            Type type = metaData.IdType;
            // According to JPA-spec all primitive types (and wrappers) are
            // supported, String, util.Date, sql.Date,
            // BigDecimal and BigInteger
            if (type == typeof(long) || type == typeof(long))
            {
                return long.TryParse(@string, out var l) ? l : throw new ArgumentException();
            }
            else if (type == typeof(string))
            {
                return @string;
            }
            else if (type == typeof(byte) || type == typeof(sbyte))
            {
                return sbyte.TryParse(@string, out var sb) ? sb : throw new ArgumentException();
            }
            else if (type == typeof(short) || type == typeof(short))
            {
                return short.TryParse(@string, out var sh) ? sh : throw new ArgumentException();
            }
            else if (type == typeof(int) || type == typeof(int))
            {
                return int.TryParse(@string, out var i) ? i : throw new ArgumentException();
            }
            else if (type == typeof(float) || type == typeof(float))
            {
                return float.TryParse(@string, out var f) ? f : throw new ArgumentException();
            }
            else if (type == typeof(double) || type == typeof(double))
            {
                return double.TryParse(@string, out var d) ? d : throw new ArgumentException();
            }
            else if (type == typeof(char) || type == typeof(char))
            {
                return new char?(@string[0]);
            }
            else if (type == typeof(DateTime))
            {
                long.TryParse(@string, out var ticks);
                return new DateTime(ticks);
            }
            else if (type == typeof(decimal))
            {
                return decimal.TryParse(@string, out var dec) ? dec : throw new ArgumentException();
            }
            else if (type == typeof(BigInteger))
            {
                return long.TryParse(@string, out var bi) ? bi : throw new ArgumentException();
            }
            else if (type == typeof(Guid))
            {
                return new Guid(@string);
            }
            else
            {
                throw new ActivitiIllegalArgumentException("Unsupported Primary key type for JPA-Entity: " + type.FullName);
            }
        }

        public virtual string GetIdString(object value)
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