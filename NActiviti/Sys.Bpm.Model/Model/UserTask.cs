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
namespace org.activiti.bpmn.model
{

    public class UserTask : Task
    {

        protected internal string assignee;
        protected internal string owner;
        protected internal string priority;
        protected internal string formKey;
        protected internal string dueDate;
        protected internal string businessCalendarName;
        protected internal string category;
        protected internal string extensionId;
        protected internal IList<string> candidateUsers = new List<string>();
        protected internal IList<string> candidateGroups = new List<string>();
        protected internal IList<FormProperty> formProperties = new List<FormProperty>();
        protected internal IList<ActivitiListener> taskListeners = new List<ActivitiListener>();
        protected internal string skipExpression;

        protected internal IDictionary<string, ISet<string>> customUserIdentityLinks = new Dictionary<string, ISet<string>>();
        protected internal IDictionary<string, ISet<string>> customGroupIdentityLinks = new Dictionary<string, ISet<string>>();

        protected internal IList<CustomProperty> customProperties = new List<CustomProperty>();

        public virtual string Assignee
        {
            get
            {
                return assignee;
            }
            set
            {
                this.assignee = value;
            }
        }


        public virtual string Owner
        {
            get
            {
                return owner;
            }
            set
            {
                this.owner = value;
            }
        }


        public virtual string Priority
        {
            get
            {
                return priority;
            }
            set
            {
                this.priority = value;
            }
        }


        public virtual string FormKey
        {
            get
            {
                return formKey;
            }
            set
            {
                this.formKey = value;
            }
        }


        public virtual string DueDate
        {
            get
            {
                return dueDate;
            }
            set
            {
                this.dueDate = value;
            }
        }


        public virtual string BusinessCalendarName
        {
            get
            {
                return businessCalendarName;
            }
            set
            {
                this.businessCalendarName = value;
            }
        }


        public virtual string Category
        {
            get
            {
                return category;
            }
            set
            {
                this.category = value;
            }
        }


        public virtual string ExtensionId
        {
            get
            {
                return extensionId;
            }
            set
            {
                this.extensionId = value;
            }
        }


        public virtual bool Extended
        {
            get
            {
                return !string.ReferenceEquals(extensionId, null) && extensionId.Length > 0;
            }
        }
        public virtual IList<string> CandidateUsers
        {
            get
            {
                return candidateUsers;
            }
            set
            {
                this.candidateUsers = value ?? new List<string>();
            }
        }


        public virtual IList<string> CandidateGroups
        {
            get
            {
                return candidateGroups;
            }
            set
            {
                this.candidateGroups = value ?? new List<string>();
            }
        }


        public virtual IList<FormProperty> FormProperties
        {
            get
            {
                return formProperties;
            }
            set
            {
                this.formProperties = value ?? new List<FormProperty>();
            }
        }


        public virtual IList<ActivitiListener> TaskListeners
        {
            get
            {
                return taskListeners;
            }
            set
            {
                this.taskListeners = value ?? new List<ActivitiListener>();
            }
        }


        public virtual void addCustomUserIdentityLink(string userId, string type)
        {
            ISet<string> userIdentitySet = customUserIdentityLinks[type];

            if (userIdentitySet == null)
            {
                userIdentitySet = new HashSet<string>();
                customUserIdentityLinks[type] = userIdentitySet;
            }

            userIdentitySet.Add(userId);
        }

        public virtual void addCustomGroupIdentityLink(string groupId, string type)
        {
            ISet<string> groupIdentitySet = customGroupIdentityLinks[type];

            if (groupIdentitySet == null)
            {
                groupIdentitySet = new HashSet<string>();
                customGroupIdentityLinks[type] = groupIdentitySet;
            }

            groupIdentitySet.Add(groupId);
        }

        public virtual IDictionary<string, ISet<string>> CustomUserIdentityLinks
        {
            get
            {
                return customUserIdentityLinks;
            }
            set
            {
                this.customUserIdentityLinks = value ?? new Dictionary<string, ISet<string>>();
            }
        }


        public virtual IDictionary<string, ISet<string>> CustomGroupIdentityLinks
        {
            get
            {
                return customGroupIdentityLinks;
            }
            set
            {
                this.customGroupIdentityLinks = value ?? new Dictionary<string, ISet<string>>();
            }
        }


        public virtual IList<CustomProperty> CustomProperties
        {
            get
            {
                return customProperties;
            }
            set
            {
                this.customProperties = value ?? new List<CustomProperty>();
            }
        }

        public virtual string SkipExpression
        {
            get
            {
                return skipExpression;
            }
            set
            {
                this.skipExpression = value;
            }
        }


        public override BaseElement clone()
        {
            UserTask clone = new UserTask();
            clone.Values = this;
            return clone;
        }

        public override BaseElement Values
        {
            set
            {
                base.Values = value;
                var val = value as UserTask;

                Assignee = val.Assignee;
                Owner = val.Owner;
                FormKey = val.FormKey;
                DueDate = val.DueDate;
                Priority = val.Priority;
                Category = val.Category;
                ExtensionId = val.ExtensionId;
                SkipExpression = val.SkipExpression;

                CandidateGroups = new List<string>(val.CandidateGroups);
                CandidateUsers = new List<string>(val.CandidateUsers);

                CustomGroupIdentityLinks = val.customGroupIdentityLinks;
                CustomUserIdentityLinks = val.customUserIdentityLinks;

                formProperties = new List<FormProperty>();
                if (val.FormProperties != null && val.FormProperties.Count > 0)
                {
                    foreach (FormProperty property in val.FormProperties)
                    {
                        formProperties.Add(property.clone() as FormProperty);
                    }
                }

                taskListeners = new List<ActivitiListener>();
                if (val.TaskListeners != null && val.TaskListeners.Count > 0)
                {
                    foreach (ActivitiListener listener in val.TaskListeners)
                    {
                        taskListeners.Add(listener.clone() as ActivitiListener);
                    }
                }
            }
        }
    }

}