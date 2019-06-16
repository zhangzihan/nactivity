using Newtonsoft.Json;

namespace Sys.Workflow.Cloud.Services.Api.Model
{

    /// <summary>
    /// 任务数据变量
    /// </summary>
    public class TaskVariable
    {

        /// <summary>
        /// 变量范围
        /// </summary>
        public enum TaskVariableScope
        {

            /// <summary>
            /// 本地
            /// </summary>
            LOCAL,

            /// <summary>
            /// 全局
            /// </summary>
            GLOBAL
        }

        private string taskId;

        private string name;

        private string type;

        private object value;

        private string executionId;

        private TaskVariableScope scope;


        /// <summary>
        /// 
        /// </summary>
        public TaskVariable()
        {

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="taskId">任务id</param>
        /// <param name="name">变量名称</param>
        /// <param name="type">变量类型</param>
        /// <param name="value">变量值</param>
        /// <param name="executionId">任务执行id</param>
        /// <param name="scope">变量作用域</param>
        //[JsonConstructor]
        public TaskVariable([JsonProperty("TaskId")]string taskId,
            [JsonProperty("Name")]string name,
            [JsonProperty("Type")]string type,
            [JsonProperty("Value")]object value,
            [JsonProperty("ExecutionId")]string executionId,
            [JsonProperty("Scope")]TaskVariableScope scope)
        {
            this.taskId = taskId;
            this.name = name;
            this.type = type;
            this.value = value;
            this.executionId = executionId;
            this.scope = scope;
        }


        /// <summary>
        /// 变量名
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
        /// 变量类型
        /// </summary>

        public virtual string Type
        {
            get
            {
                return type;
            }
            set => type = value;
        }

        /// <summary>
        /// 变量值
        /// </summary>
        public virtual object Value
        {
            get
            {
                return value;
            }
            set => this.value = value;
        }

        /// <summary>
        /// 任务执行id
        /// </summary>

        public virtual string ExecutionId
        {
            get
            {
                return executionId;
            }
            set => executionId = value;
        }

        /// <summary>
        /// 任务id
        /// </summary>

        public virtual string TaskId
        {
            get
            {
                return taskId;
            }
            set => taskId = value;
        }

        /// <summary>
        /// 变量作用域范围
        /// </summary>

        public virtual TaskVariableScope Scope
        {
            get
            {
                return scope;
            }
            set => scope = value;
        }
    }

}