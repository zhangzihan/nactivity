namespace org.activiti.cloud.services.api.model
{
    using Newtonsoft.Json;


    /// <summary>
    /// 任务分配用户
    /// </summary>
    public class TaskCandidateUser
    {
        private string userId;
        private string taskId;


        /// <summary>
        /// 
        /// </summary>
        public TaskCandidateUser()
        {

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="userId">用户id</param>
        /// <param name="taskId">任务id</param>
        //[JsonConstructor]
        public TaskCandidateUser([JsonProperty("UserId")]string userId, [JsonProperty("TaskId")]string taskId)
        {
            this.userId = userId;
            this.taskId = taskId;
        }


        /// <summary>
        /// 用户id
        /// </summary>
        public virtual string UserId
        {
            get
            {
                return userId;
            }
            set => userId = value;
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