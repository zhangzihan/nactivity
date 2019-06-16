using Newtonsoft.Json;

namespace Sys.Workflow.cloud.services.api.model
{

    /// <summary>
    /// 流程变量
    /// </summary>
    public class HistoricVariableInstance
    {

        private string processInstanceId;

        private string name;

        private string type;

        private object value;

        private string taskId;


        /// <summary>
        /// 
        /// </summary>
        public HistoricVariableInstance()
        {

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="processInstanceId">实例id</param>
        /// <param name="name">变量名称</param>
        /// <param name="type">变量类型</param>
        /// <param name="value">变量值</param>
        /// <param name="taskId">流程任务id</param>
        //[JsonConstructor]
        public HistoricVariableInstance([JsonProperty("ProcessInstanceId")]string processInstanceId,
            [JsonProperty("Name")]string name,
            [JsonProperty("Type")]string type,
            [JsonProperty("Value")]object value,
            [JsonProperty("TaskId")]string taskId)
        {
            this.name = name;
            this.type = type;
            this.value = value;
            this.taskId = taskId;
            this.processInstanceId = processInstanceId;
        }

        /// <summary>
        /// 变量名称
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
        /// 流程实例id
        /// </summary>

        public virtual string ProcessInstanceId
        {
            get
            {
                return processInstanceId;
            }
            set => processInstanceId = value;
        }

        /// <summary>
        /// 流程执行id
        /// </summary>

        public virtual string TaskId
        {
            get
            {
                return taskId;
            }
            set => taskId = value;
        }
    }

}