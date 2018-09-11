using Newtonsoft.Json;
using System.Collections.Generic;

namespace org.activiti.cloud.services.api.model
{
    public class ProcessDefinitionMeta
    {
        private string id;
        private string name;
        private string description;
        private int version;
        private ISet<string> users;
        private ISet<string> groups;
        private ISet<ProcessDefinitionVariable> variables;
        private ISet<ProcessDefinitionUserTask> userTasks;
        private ISet<ProcessDefinitionServiceTask> serviceTasks;

        public ProcessDefinitionMeta()
        {
        }

        [JsonConstructor]
        public ProcessDefinitionMeta([JsonProperty("Id")]string id,
            [JsonProperty("Name")]string name,
            [JsonProperty("Description")]string description,
            [JsonProperty("Version")]int version,
            [JsonProperty("Users")]ISet<string> users,
            [JsonProperty("Groups")]ISet<string> groups,
            [JsonProperty("Variables")]ISet<ProcessDefinitionVariable> variables,
            [JsonProperty("UserTasks")]ISet<ProcessDefinitionUserTask> userTasks,
            [JsonProperty("ServiceTasks")]ISet<ProcessDefinitionServiceTask> serviceTasks) : base()
        {
            this.id = id;
            this.name = name;
            this.description = description;
            this.version = version;
            this.users = users;
            this.groups = groups;
            this.variables = variables;
            this.userTasks = userTasks;
            this.serviceTasks = serviceTasks;
        }

        public virtual string Id
        {
            get
            {
                return id;
            }
        }

        public virtual string Name
        {
            get
            {
                return name;
            }
        }

        public virtual string Description
        {
            get
            {
                return description;
            }
        }

        public virtual int Version
        {
            get
            {
                return version;
            }
        }

        public virtual ISet<string> Users
        {
            get
            {
                return users;
            }
        }

        public virtual ISet<string> Groups
        {
            get
            {
                return groups;
            }
        }

        public virtual ISet<ProcessDefinitionVariable> Variables
        {
            get
            {
                return variables;
            }
        }

        public virtual ISet<ProcessDefinitionUserTask> UserTasks
        {
            get
            {
                return userTasks;
            }
        }

        public virtual ISet<ProcessDefinitionServiceTask> ServiceTasks
        {
            get
            {
                return serviceTasks;
            }
        }

    }

}