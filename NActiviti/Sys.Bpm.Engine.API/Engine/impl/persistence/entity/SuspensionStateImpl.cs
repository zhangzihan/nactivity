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
    public class SuspensionStateImpl : ISuspensionState
    {

        private int stateCode;
        private string name;

        public SuspensionStateImpl()
        {

        }

        public SuspensionStateImpl(int suspensionCode, string name)
        {
            this.stateCode = suspensionCode;
            this.name = name;
        }

        public virtual int StateCode
        {
            get
            {
                return stateCode;
            }
            set
            {
                stateCode = value;
            }
        }

        public string Name
        {
            get => name;
            set => name = value;
        }

        public override int GetHashCode()
        {
            const int prime = 31;
            int result = 1;
            result = prime * result + stateCode;
            return result;
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj == null)
            {
                return false;
            }
            if (this.GetType() != obj.GetType())
            {
                return false;
            }
            SuspensionStateImpl other = (SuspensionStateImpl)obj;
            if (stateCode != other.stateCode)
            {
                return false;
            }
            return true;
        }

        public override string ToString()
        {
            return name;
        }
    }

}