using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

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
    [Serializable]
    public class CommentEntityImpl : AbstractEntityNoRevision, ICommentEntity
    {

        private const long serialVersionUID = 1L;

        // If comments would be removable, revision needs to be added!

        protected internal string type;
        protected internal string userId;
        protected internal DateTime? time;
        protected internal string taskId;
        protected internal string processInstanceId;
        protected internal string action;
        protected internal string message;
        protected internal string fullMessage;

        public CommentEntityImpl()
        {

        }

        public override PersistentState PersistentState
        {
            get
            {
                return new PersistentState();// typeof(CommentEntityImpl);
            }
        }

        public virtual byte[] FullMessageBytes
        {
            get
            {
                return fullMessage?.GetBytes();
            }
            set
            {
                fullMessage = value is object ? StringHelper.NewString(value) : null;
            }
        }


        public static string MESSAGE_PARTS_MARKER = "_|_";
        public static readonly Regex MESSAGE_PARTS_MARKER_REGEX = new Regex("_\\|_");

        public virtual string Message
        {
            set
            {
                message = value;
            }
            get
            {
                return message;
            }
        }

        public virtual IList<string> MessageParts
        {
            get
            {
                if (message is null)
                {
                    return null;
                }
                IList<string> messageParts = new List<string>();

                string[] parts = MESSAGE_PARTS_MARKER_REGEX.Split(message);
                foreach (string part in parts)
                {
                    if ("null".Equals(part))
                    {
                        messageParts.Add(null);
                    }
                    else
                    {
                        messageParts.Add(part);
                    }
                }
                return messageParts;
            }
            set
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (string part in value)
                {
                    if (part is object)
                    {
                        stringBuilder.Append(part.Replace(MESSAGE_PARTS_MARKER, " | "));
                        stringBuilder.Append(MESSAGE_PARTS_MARKER);
                    }
                    else
                    {
                        stringBuilder.Append("null");
                        stringBuilder.Append(MESSAGE_PARTS_MARKER);
                    }
                }
                for (int i = 0; i < MESSAGE_PARTS_MARKER.Length; i++)
                {
                    stringBuilder.Remove(stringBuilder.Length - 1, 1);
                }
                message = stringBuilder.ToString();
            }
        }

        // getters and setters
        // //////////////////////////////////////////////////////

        public virtual string UserId
        {
            get
            {
                return userId;
            }
            set
            {
                this.userId = value;
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

        public virtual DateTime? Time
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


        public virtual string FullMessage
        {
            get
            {
                return fullMessage;
            }
            set
            {
                this.fullMessage = value;
            }
        }


        public virtual string Action
        {
            get
            {
                return action;
            }
            set
            {
                this.action = value;
            }
        }
    }
}