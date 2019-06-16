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

namespace Sys.Workflow.engine.impl.persistence.entity
{

    using Sys.Workflow.engine.impl.db;
    using Sys.Workflow.engine.impl.variable;

    /// 
    /// 
    [Serializable]
    public class HistoricVariableInstanceEntityImpl : AbstractEntity, IHistoricVariableInstanceEntity, IBulkDeleteable
    {

        private const long serialVersionUID = 1L;

        protected internal string name;
        protected internal IVariableType variableType;

        protected internal string processInstanceId;
        protected internal string executionId;
        protected internal string taskId;

        protected internal DateTime? createTime;
        protected internal DateTime? lastUpdatedTime;

        protected internal long? longValue;
        protected internal double? doubleValue;
        protected internal string textValue;
        protected internal string textValue2;
        protected internal ByteArrayRef byteArrayRef;

        protected internal object cachedValue;

        public HistoricVariableInstanceEntityImpl()
        {

        }

        public override PersistentState PersistentState
        {
            get
            {
                PersistentState persistentState = new PersistentState
                {
                    ["textValue"] = textValue,
                    ["textValue2"] = textValue2,
                    ["doubleValue"] = doubleValue,
                    ["longValue"] = longValue
                };

                if (byteArrayRef != null)
                {
                    persistentState["byteArrayRef"] = byteArrayRef.Id;
                }

                persistentState["createTime"] = createTime;
                persistentState["lastUpdatedTime"] = lastUpdatedTime;

                return persistentState;
            }
        }

        public virtual object Value
        {
            get
            {
                if (!variableType.Cachable || cachedValue == null)
                {
                    cachedValue = variableType.GetValue(this);
                }
                return cachedValue;
            }
        }

        // byte array value /////////////////////////////////////////////////////////

        public virtual byte[] Bytes
        {
            get
            {
                if (byteArrayRef != null)
                {
                    return byteArrayRef.Bytes;
                }
                return null;
            }
            set
            {
                if (byteArrayRef == null)
                {
                    byteArrayRef = new ByteArrayRef();
                }
                byteArrayRef.SetValue("hist.var-" + name, value);
            }
        }


        // getters and setters //////////////////////////////////////////////////////

        public virtual string VariableTypeName
        {
            get
            {
                return (variableType?.TypeName);
            }
        }

        public virtual string VariableName
        {
            get
            {
                return name;
            }
        }

        public virtual IVariableType VariableType
        {
            get
            {
                return variableType;
            }
            set
            {
                this.variableType = value;
            }
        }


        public virtual string Name
        {
            get
            {
                return name;
            }
            set
            {
                this.name = value;
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


        public virtual DateTime? CreateTime
        {
            get
            {
                return createTime;
            }
            set
            {
                this.createTime = value;
            }
        }


        public virtual DateTime? LastUpdatedTime
        {
            get
            {
                return lastUpdatedTime;
            }
            set
            {
                this.lastUpdatedTime = value;
            }
        }


        public virtual string ExecutionId
        {
            get
            {
                return executionId;
            }
            set
            {
                this.executionId = value;
            }
        }


        public virtual DateTime? Time
        {
            get
            {
                return CreateTime;
            }
        }

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

        // common methods //////////////////////////////////////////////////////////

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("HistoricVariableInstanceEntity[");
            sb.Append("id=").Append(id);
            sb.Append(", name=").Append(name);
            sb.Append(", revision=").Append(revision);
            sb.Append(", type=").Append(variableType != null ? variableType.TypeName : "null");
            if (longValue != null)
            {
                sb.Append(", longValue=").Append(longValue);
            }
            if (doubleValue != null)
            {
                sb.Append(", doubleValue=").Append(doubleValue);
            }
            if (!(textValue is null))
            {
                sb.Append(", textValue=").Append(textValue.PadLeft(40, ' '));
            }
            if (!(textValue2 is null))
            {
                sb.Append(", textValue2=").Append(textValue2.PadLeft(40, ' '));
            }
            if (byteArrayRef != null && !(byteArrayRef.Id is null))
            {
                sb.Append(", byteArrayValueId=").Append(byteArrayRef.Id);
            }
            sb.Append("]");
            return sb.ToString();
        }

    }

}