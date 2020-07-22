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
    using Newtonsoft.Json.Linq;
    using Sys.Workflow.Engine.Impl.DB;
    using System.Collections.Generic;

    /// <summary>
    /// Abstract superclass for the common properties of all <seealso cref="IEntity"/> implementations.
    /// 
    /// 
    /// </summary>
    public abstract class AbstractEntity : IEntity, IHasRevision
    {
        public virtual PersistentState PersistentState
        {
            get
            {
                return new PersistentState()
                {
                    ["id"] = this.id,
                    ["revision"] = this.revision,
                };
            }
        }

        private string id;
        private int revision = 1;

        private bool isInserted;
        private bool isUpdated;
        private bool isDeleted;

        public virtual string Id
        {
            get
            {
                return id;
            }
            set
            {
                this.id = value;
            }
        }


        public virtual int RevisionNext
        {
            get
            {
                return revision + 1;
            }
        }

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


        public virtual bool Inserted
        {
            get
            {
                return isInserted;
            }
            set
            {
                this.isInserted = value;
            }
        }


        public virtual bool Updated
        {
            get
            {
                return isUpdated;
            }
            set
            {
                this.isUpdated = value;
            }
        }


        public virtual bool Deleted
        {
            get
            {
                return isDeleted;
            }
            set
            {
                this.isDeleted = value;
            }
        }


    }

}