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

namespace Sys.Workflow.engine.impl.persistence.entity
{

    /// 
    [Serializable]
    public class AttachmentEntityImpl : AbstractEntity, IAttachmentEntity
    {

        private const long serialVersionUID = 1L;

        protected internal string name;
        protected internal string description;
        protected internal string type;
        protected internal string taskId;
        protected internal string processInstanceId;
        protected internal string url;
        protected internal string contentId;
        protected internal IByteArrayEntity content;
        protected internal string userId;
        protected internal DateTime time;

        public AttachmentEntityImpl()
        {

        }

        public override PersistentState PersistentState
        {
            get
            {
                PersistentState persistentState = new PersistentState
                {
                    ["name"] = name,
                    ["description"] = description
                };
                return persistentState;
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


        public virtual string Description
        {
            get
            {
                return description;
            }
            set
            {
                this.description = value;
            }
        }


        public virtual string Type
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


        public virtual string ProcessInstanceId
        {
            get
            {
                return processInstanceId;
            }
            set
            {
                this.processInstanceId = value;
            }
        }


        public virtual string Url
        {
            get
            {
                return url;
            }
            set
            {
                this.url = value;
            }
        }


        public virtual string ContentId
        {
            get
            {
                return contentId;
            }
            set
            {
                this.contentId = value;
            }
        }


        public virtual IByteArrayEntity Content
        {
            get
            {
                return content;
            }
            set
            {
                this.content = value;
            }
        }


        public virtual string UserId
        {
            set
            {
                this.userId = value;
            }
            get
            {
                return userId;
            }
        }


        public virtual DateTime Time
        {
            get
            {
                return time;
            }
            set
            {
                this.time = value;
            }
        }


    }

}