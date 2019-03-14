using Newtonsoft.Json;
using System.Collections.Generic;

namespace org.activiti.cloud.services.api.model
{

    /// <summary>
    /// 流程定义元数据
    /// </summary>
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


        /// <summary>
        /// 构造函数
        /// </summary>
        public ProcessDefinitionMeta()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="name">名称</param>
        /// <param name="description">描述</param>
        /// <param name="version">版本</param>
        /// <param name="users">用户列表</param>
        /// <param name="groups">用户组列表</param>
        /// <param name="variables">数据变量列表</param>
        /// <param name="userTasks">用户任务列表</param>
        /// <param name="serviceTasks">服务任务列表</param>

        //[JsonConstructor]
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


        /// <summary>
        /// id
        /// </summary>
        public virtual string Id
        {
            get
            {
                return id;
            }
            set => id = value;
        }

        /// <summary>
        /// 名称
        /// </summary>

        public virtual string Name
        {
            get
            {
                return name;
            }
            set => name = value;
        }

        /// <summary>
        /// 描述
        /// </summary>

        public virtual string Description
        {
            get
            {
                return description;
            }
            set => description = value;
        }

        /// <summary>
        /// 版本
        /// </summary>

        public virtual int Version
        {
            get
            {
                return version;
            }
            set => version = value;
        }

        /// <summary>
        /// 用户列表
        /// </summary>

        public virtual ISet<string> Users
        {
            get
            {
                return users;
            }
            set => users = value;
        }

        /// <summary>
        /// 用户组列表
        /// </summary>

        public virtual ISet<string> Groups
        {
            get
            {
                return groups;
            }
            set => groups = value;
        }

        /// <summary>
        /// 数据变量列表
        /// </summary>

        public virtual ISet<ProcessDefinitionVariable> Variables
        {
            get
            {
                return variables;
            }
            set => variables = value;
        }

        /// <summary>
        /// 用户任务列表
        /// </summary>

        public virtual ISet<ProcessDefinitionUserTask> UserTasks
        {
            get
            {
                return userTasks;
            }
            set => userTasks = value;
        }

        /// <summary>
        /// 服务任务列表
        /// </summary>

        public virtual ISet<ProcessDefinitionServiceTask> ServiceTasks
        {
            get
            {
                return serviceTasks;
            }
            set => serviceTasks = value;
        }

    }

}