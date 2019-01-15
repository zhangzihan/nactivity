using System;
using System.Collections.Generic;
using System.Text;

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
namespace org.activiti.engine.impl.persistence.entity
{

    using org.activiti.engine.impl.db;
    using org.activiti.engine.impl.variable;

    /// 
    /// 
    /// 
    [Serializable]
    public class VariableInstanceEntityImpl : AbstractEntity, IVariableInstanceEntity, IValueFields, IBulkDeleteable
    {

        private const long serialVersionUID = 1L;

        protected internal string name;
        protected internal IVariableType type;
        protected internal string typeName;

        protected internal string processInstanceId;
        protected internal string executionId;
        protected internal string taskId;

        protected internal long? longValue;
        protected internal double? doubleValue;
        protected internal string textValue;
        protected internal string textValue2;
        protected internal ByteArrayRef byteArrayRef;

        protected internal object cachedValue;
        protected internal bool forcedUpdate;
        protected internal bool deleted;

        public VariableInstanceEntityImpl()
        {

        }

        public override PersistentState PersistentState
        {
            get
            {
                PersistentState persistentState = new PersistentState();
                if (longValue != null)
                {
                    persistentState["longValue"] = longValue;
                }
                if (doubleValue != null)
                {
                    persistentState["doubleValue"] = doubleValue;
                }
                if (!string.ReferenceEquals(textValue, null))
                {
                    persistentState["textValue"] = textValue;
                }
                if (!string.ReferenceEquals(textValue2, null))
                {
                    persistentState["textValue2"] = textValue2;
                }
                if (byteArrayRef != null && !string.ReferenceEquals(byteArrayRef.Id, null))
                {
                    persistentState["byteArrayValueId"] = byteArrayRef.Id;
                }
                if (forcedUpdate)
                {
                    persistentState["forcedUpdate"] = true;
                }
                return persistentState;
            }
        }

        public virtual IExecutionEntity Execution
        {
            set
            {
                this.executionId = value.Id;
                this.processInstanceId = value.ProcessInstanceId;
                forceUpdate();
            }
        }

        public virtual void forceUpdate()
        {
            forcedUpdate = true;
        }

        public virtual string ProcessInstanceId
        {
            set
            {
                this.processInstanceId = value;
            }
            get
            {
                return processInstanceId;
            }
        }

        public virtual string ExecutionId
        {
            set
            {
                this.executionId = value;
            }
            get
            {
                return executionId;
            }
        }

        // byte array value ///////////////////////////////////////////////////////////

        public virtual byte[] Bytes
        {
            get
            {
                ensureByteArrayRefInitialized();
                return byteArrayRef.Bytes;
            }
            set
            {
                ensureByteArrayRefInitialized();
                byteArrayRef.setValue("var-" + name, value);
            }
        }


        public virtual ByteArrayRef ByteArrayRef
        {
            get
            {
                return byteArrayRef;
            }
            set
            {
                byteArrayRef = value;
            }
        }

        protected internal virtual void ensureByteArrayRefInitialized()
        {
            if (byteArrayRef == null)
            {
                byteArrayRef = new ByteArrayRef();
            }
        }

        // value //////////////////////////////////////////////////////////////////////

        public virtual object Value
        {
            get
            {
                if (!type.Cachable || cachedValue == null)
                {
                    cachedValue = type.getValue(this);
                }
                return cachedValue;
            }
            set
            {
                type.setValue(value, this);
                typeName = type.TypeName;
                cachedValue = value;
            }
        }


        // getters and setters ////////////////////////////////////////////////////////

        public virtual string Name
        {
            set
            {
                this.name = value;
            }
            get
            {
                return name;
            }
        }


        public virtual string TypeName
        {
            get
            {
                if (!string.ReferenceEquals(typeName, null))
                {
                    return typeName;
                }
                else if (type != null)
                {
                    return type.TypeName;
                }
                else
                {
                    return typeName;
                }
            }
            set
            {
                this.typeName = value;
            }
        }

        public virtual IVariableType Type
        {
            get
            {
                return type;
            }
            set
            {
                this.type = value;
            }
        }



        public virtual string TaskId
        {
            get
            {
                return taskId;
            }
            set
            {
                this.taskId = value;
            }
        }



        public virtual long? LongValue
        {
            get
            {
                return longValue;
            }
            set
            {
                this.longValue = value;
            }
        }


        public virtual double? DoubleValue
        {
            get
            {
                return doubleValue;
            }
            set
            {
                this.doubleValue = value;
            }
        }


        public virtual string TextValue
        {
            get
            {
                return textValue;
            }
            set
            {
                this.textValue = value;
            }
        }


        public virtual string TextValue2
        {
            get
            {
                return textValue2;
            }
            set
            {
                this.textValue2 = value;
            }
        }


        public virtual object CachedValue
        {
            get
            {
                return cachedValue;
            }
            set
            {
                this.cachedValue = value;
            }
        }


        // misc methods ///////////////////////////////////////////////////////////////

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("VariableInstanceEntity[");
            sb.Append("id=").Append(id);
            sb.Append(", name=").Append(name);
            sb.Append(", type=").Append(type != null ? type.TypeName : "null");
            if (longValue != null)
            {
                sb.Append(", longValue=").Append(longValue);
            }
            if (doubleValue != null)
            {
                sb.Append(", doubleValue=").Append(doubleValue);
            }
            if (!string.ReferenceEquals(textValue, null))
            {
                sb.Append(", textValue=").Append(textValue.PadLeft(40, ' '));
            }
            if (!string.ReferenceEquals(textValue2, null))
            {
                sb.Append(", textValue2=").Append(textValue2.PadLeft(40, ' '));
            }
            if (byteArrayRef != null && !string.ReferenceEquals(byteArrayRef.Id, null))
            {
                sb.Append(", byteArrayValueId=").Append(byteArrayRef.Id);
            }
            sb.Append("]");
            return sb.ToString();
        }

    }

}