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

    /// <summary>
    /// Job entity.
    /// 
    /// 
    /// 
    /// </summary>
    [Serializable]
    public class JobEntityImpl : AbstractJobEntityImpl, IJobEntity
    {
        private const long serialVersionUID = 1L;
        protected internal string lockOwner;
        protected internal DateTime? lockExpirationTime;

        public override PersistentState PersistentState
        {
            get
            {
                PersistentState persistentState = (PersistentState)base.PersistentState;
                persistentState["lockOwner"] = lockOwner;
                persistentState["lockExpirationTime"] = lockExpirationTime;

                return persistentState;
            }
        }

        // getters and setters ////////////////////////////////////////////////////////

        public override IExecutionEntity Execution
        {
            set
            {
                base.Execution = value;
                value.Jobs.Add(this);
            }
        }

        public virtual string LockOwner
        {
            get
            {
                return lockOwner;
            }
            set
            {
                this.lockOwner = value;
            }
        }

        public override int Revision
        {
            get
            {
                return base.Revision;
            }
            set
            {
                base.Revision = value;
            }
        }

        public override int RevisionNext
        {
            get
            {
                return base.RevisionNext;
            }
        }

        public virtual DateTime? LockExpirationTime
        {
            get
            {
                return lockExpirationTime;
            }
            set
            {
                this.lockExpirationTime = value;
            }
        }


        public override string ToString()
        {
            return "JobEntity [id=" + id + "]";
        }

    }

}