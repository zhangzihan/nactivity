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
namespace Sys.Workflow.Engine.Impl.Persistence.Entity
{
    /// 
    /// 
    /// 
    [Serializable]
    public class ByteArrayEntityImpl : AbstractEntity, IByteArrayEntity
    {

        private const long serialVersionUID = 1L;

        protected internal string name;
        protected internal byte[] bytes;
        protected internal string deploymentId;

        public ByteArrayEntityImpl()
        {

        }

        public virtual byte[] Bytes
        {
            get
            {
                return bytes;
            }
            set
            {
                this.bytes = value;
            }
        }

        public override PersistentState PersistentState => new PersistentState_(name, bytes);

        // getters and setters ////////////////////////////////////////////////////////

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


        public virtual string DeploymentId
        {
            get
            {
                return deploymentId;
            }
            set
            {
                this.deploymentId = value;
            }
        }



        public override string ToString()
        {
            return "ByteArrayEntity[id=" + Id + ", name=" + name + ", size=" + (bytes is not null ? bytes.Length : 0) + "]";
        }

        // Wrapper for a byte array, needed to do byte array comparisons
        // See https://activiti.atlassian.net/browse/ACT-1524
        private class PersistentState_ : PersistentState
        {

            internal readonly string name;
            internal readonly byte[] bytes;

            public PersistentState_(string name, byte[] bytes)
            {
                this.name = name;
                this.bytes = bytes;
            }

            public override bool Equals(object obj)
            {
                if (obj is PersistentState_ other)
                {
                    return this.name == other.name && this.bytes.Equals(other.bytes);
                }
                return false;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }
    }

}