using Newtonsoft.Json;

namespace org.activiti.cloud.services.api.model
{

    /// <summary>
    /// 任务分配组
    /// </summary>
    public class TaskCandidateGroup
    {
        private string groupId;
        private string taskId;


        /// <summary>
        /// 
        /// </summary>
        public TaskCandidateGroup()
        {

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="groupId">组id</param>
        /// <param name="taskId">任务id</param>
        //[JsonConstructor]
        public TaskCandidateGroup([JsonProperty("GroupId")]string groupId, [JsonProperty("TaskId")]string taskId)
        {
            this.groupId = groupId;
            this.taskId = taskId;
        }



        /// <summary>
        /// 组id
        /// </summary>
        public virtual string GroupId
        {
            get
            {
                return groupId;
            }
            set => groupId = value;
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

    }

}