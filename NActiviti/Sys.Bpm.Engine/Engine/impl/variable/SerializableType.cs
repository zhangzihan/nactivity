using System;

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
    using Newtonsoft.Json;
    using Newtonsoft.Json.Bson;
    using Newtonsoft.Json.Linq;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;

    /// 
    /// 
    public class SerializableType : ByteArrayType
    {

        public const string TYPE_NAME = "serializable";

        protected internal bool trackDeserializedObjects;

        public override string TypeName
        {
            get
            {
                return TYPE_NAME;
            }
        }

        public SerializableType()
        {

        }

        public SerializableType(bool trackDeserializedObjects)
        {
            this.trackDeserializedObjects = trackDeserializedObjects;
        }

        public override object getValue(IValueFields valueFields)
        {
            object cachedObject = valueFields.CachedValue;
            if (cachedObject != null)
            {
                return cachedObject;
            }

            byte[] bytes = (byte[])base.getValue(valueFields);
            if (bytes != null)
            {
                object deserializedObject = deserialize(bytes, valueFields);
                valueFields.CachedValue = deserializedObject;

                if (trackDeserializedObjects && valueFields is IVariableInstanceEntity)
                {
                    Context.CommandContext.addCloseListener(new VerifyDeserializedObjectCommandContextCloseListener(new DeserializedObject(this, valueFields.CachedValue, bytes, (IVariableInstanceEntity)valueFields)));
                }

                return deserializedObject;
            }
            return null; // byte array is null
        }

        public override void setValue(object value, IValueFields valueFields)
        {
            byte[] bytes = serialize(value, valueFields);
            valueFields.CachedValue = value;

            base.setValue(bytes, valueFields);

            if (trackDeserializedObjects && valueFields is IVariableInstanceEntity)
            {
                Context.CommandContext.addCloseListener(new VerifyDeserializedObjectCommandContextCloseListener(new DeserializedObject(this, valueFields.CachedValue, bytes, (IVariableInstanceEntity)valueFields)));
            }

        }

        public virtual byte[] serialize(object value, IValueFields valueFields)
        {
            if (value == null)
            {
                return null;
            }

            try
            {
                string s = JsonConvert.SerializeObject(value, Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        TypeNameHandling = TypeNameHandling.All
                    });
                return Encoding.UTF8.GetBytes(s);
                //using (MemoryStream ms = new MemoryStream())
                //{
                //    using (BsonDataWriter datawriter = new BsonDataWriter(ms))
                //    {
                //JsonSerializer serializer = new JsonSerializer();
                //serializer.Serialize(datawriter, value);
                //return ms.ToArray();
                //    }
                //}
            }
            catch (Exception e)
            {
                throw new ActivitiException("Couldn't serialize value '" + value + "' in variable '" + valueFields.Name + "'", e);
            }
        }

        public virtual object deserialize(byte[] bytes, IValueFields valueFields)
        {
            try
            {
                string s = Encoding.UTF8.GetString(bytes);
                return JsonConvert.DeserializeObject(s, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    TypeNameHandling = TypeNameHandling.All
                });

                //using (MemoryStream ms = new MemoryStream(bytes))
                //{
                //    using (BsonDataReader reader = new BsonDataReader(ms))
                //    {
                //        JsonSerializer serializer = new JsonSerializer();
                //        return serializer.Deserialize(reader);
                //    }
                //}

                //    string value = Encoding.UTF8.GetString(bytes);
                //    object deserializedObject = JsonConvert.DeserializeObject(value, new JsonSerializerSettings
                //    {
                //        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                //        TypeNameHandling = TypeNameHandling.All
                //    });

                //    return deserializedObject;
            }
            catch (Exception e)
            {
                throw new ActivitiException("Couldn't deserialize object in variable '" + valueFields.Name + "'", e);
            }
        }

        public override bool isAbleToStore(object value)
        {
            // TODO don't we need null support here?
            //return value is Serializable;
            return true;
        }
    }

}