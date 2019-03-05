using System;
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
    using org.activiti.engine.impl.variable;

    /// 
    /// 
    [Serializable]
    public class HistoricDetailVariableInstanceUpdateEntityImpl : HistoricDetailEntityImpl, IHistoricDetailVariableInstanceUpdateEntity
    {

        private const long serialVersionUID = 1L;

        protected internal int revision;

        protected internal string name;
        protected internal IVariableType variableType;

        protected internal long? longValue;
        protected internal double? doubleValue;
        protected internal string textValue;
        protected internal string textValue2;
        protected internal ByteArrayRef byteArrayRef;

        protected internal object cachedValue;

        public HistoricDetailVariableInstanceUpdateEntityImpl()
        {
            this.detailType = "VariableUpdate";
        }

        public override PersistentState PersistentState
        {
            get
            {
                // HistoricDetailVariableInstanceUpdateEntity is immutable, so always
                // the same object is returned
                return new PersistentState();// typeof(HistoricDetailVariableInstanceUpdateEntityImpl);
            }
        }

        public virtual object Value
        {
            get
            {
                if (!variableType.Cachable || cachedValue == null)
                {
                    cachedValue = variableType.getValue(this);
                }
                return cachedValue;
            }
        }

        public virtual string VariableTypeName
        {
            get
            {
                return (variableType != null ? variableType.TypeName : null);
            }
        }

        public virtual int RevisionNext
        {
            get
            {
                return revision + 1;
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
                string byteArrayName = "hist.detail.var-" + name;
                if (byteArrayRef == null)
                {
                    byteArrayRef = new ByteArrayRef();
                }
                byteArrayRef.setValue(byteArrayName, value);
            }
        }

        public virtual IByteArrayRef ByteArrayRef
        {
            get
            {
                return byteArrayRef;
            }
        }


        // getters and setters ////////////////////////////////////////////////////////

        public virtual int Revision
        {
            get
            {
                return revision;
            }
            set
            {
                this.revision = value;
            }
        }


        public virtual string VariableName
        {
            get
            {
                return name;
            }
        }

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


        // common methods ///////////////////////////////////////////////////////////////

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("HistoricDetailVariableInstanceUpdateEntity[");
            sb.Append("id=").Append(id);
            sb.Append(", name=").Append(name);
            sb.Append(", type=").Append(variableType != null ? variableType.TypeName : "null");
            if (longValue != null)
            {
                sb.Append(", longValue=").Append(longValue);
            }
            if (doubleValue != null)
            {
                sb.Append(", doubleValue=").Append(doubleValue);
            }
            if (!ReferenceEquals(textValue, null))
            {
                sb.Append(", textValue=").Append(textValue.PadLeft(40, ' '));
            }
            if (!ReferenceEquals(textValue2, null))
            {
                sb.Append(", textValue2=").Append(textValue2.PadLeft(40, ' '));
            }
            if (byteArrayRef != null && !ReferenceEquals(byteArrayRef.Id, null))
            {
                sb.Append(", byteArrayValueId=").Append(byteArrayRef.Id);
            }
            sb.Append("]");
            return sb.ToString();
        }

    }

}