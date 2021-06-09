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
namespace Sys.Workflow.Engine.Impl.Persistence.Entity
{

    using Sys.Workflow.Engine.Impl.DB;
    using Sys.Workflow.Engine.Impl.Variable;

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class VariableInstanceEntityImpl : AbstractEntity, IVariableInstanceEntity, IValueFields, IBulkDeleteable
    {
        private const long serialVersionUID = 1L;

        private string name;
        private IVariableType type;
        private string typeName;

        private string processInstanceId;
        private string executionId;
        private string taskId;

        private long? longValue;
        private double? doubleValue;
        private string textValue;
        private string textValue2;
        private ByteArrayRef byteArrayRef;

        private object cachedValue;
        private bool forcedUpdate;

        /// <summary>
        /// 
        /// </summary>
        public VariableInstanceEntityImpl()
        {

        }
        /// <summary>
        /// 
        /// </summary>
        public override PersistentState PersistentState
        {
            get
            {
                PersistentState persistentState = new PersistentState();
                if (longValue is object)
                {
                    persistentState["longValue"] = longValue;
                }
                if (doubleValue is object)
                {
                    persistentState["doubleValue"] = doubleValue;
                }
                if (textValue is object)
                {
                    persistentState["textValue"] = textValue;
                }
                if (textValue2 is object)
                {
                    persistentState["textValue2"] = textValue2;
                }
                if (byteArrayRef is object && byteArrayRef.Id is object)
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
        /// <summary>
        /// 
        /// </summary>
        public virtual IExecutionEntity Execution
        {
            set
            {
                this.executionId = value.Id;
                this.processInstanceId = value.ProcessInstanceId;
                ForceUpdate();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual void ForceUpdate()
        {
            forcedUpdate = true;
        }
        /// <summary>
        /// 
        /// </summary>
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
        /// <summary>
        /// 
        /// </summary>
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
        /// <summary>
        /// 
        /// </summary>
        public virtual byte[] Bytes
        {
            get
            {
                EnsureByteArrayRefInitialized();
                return byteArrayRef.Bytes;
            }
            set
            {
                EnsureByteArrayRefInitialized();
                byteArrayRef.SetValue("var-" + name, value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IByteArrayRef ByteArrayRef
        {
            get
            {
                return byteArrayRef;
            }
            set
            {
                byteArrayRef = value as ByteArrayRef;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected internal virtual void EnsureByteArrayRefInitialized()
        {
            if (byteArrayRef is null)
            {
                byteArrayRef = new ByteArrayRef();
            }
        }

        // value //////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 
        /// </summary>
        public virtual object Value
        {
            get
            {
                if (!type.Cachable || cachedValue is null)
                {
                    cachedValue = type.GetValue(this);
                }
                return cachedValue;
            }
            set
            {
                type.SetValue(value, this);
                typeName = type.TypeName;
                cachedValue = value;
            }
        }


        // getters and setters ////////////////////////////////////////////////////////
        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
        public virtual string TypeName
        {
            get
            {
                if (typeName is object)
                {
                    return typeName;
                }
                else if (type is object)
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
        /// <summary>
        /// 
        /// </summary>
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


        /// <summary>
        /// 
        /// </summary>
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


        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
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
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("VariableInstanceEntity[");
            sb.Append("id=").Append(Id);
            sb.Append(", name=").Append(name);
            sb.Append(", type=").Append(type is object ? type.TypeName : "null");
            if (longValue is object)
            {
                sb.Append(", longValue=").Append(longValue);
            }
            if (doubleValue is object)
            {
                sb.Append(", doubleValue=").Append(doubleValue);
            }
            if (textValue is object)
            {
                sb.Append(", textValue=").Append(textValue.PadLeft(40, ' '));
            }
            if (textValue2 is object)
            {
                sb.Append(", textValue2=").Append(textValue2.PadLeft(40, ' '));
            }
            if (byteArrayRef is object && byteArrayRef.Id is object)
            {
                sb.Append(", byteArrayValueId=").Append(byteArrayRef.Id);
            }
            sb.Append("]");
            return sb.ToString();
        }
    }
}