namespace org.activiti.cloud.services.api.model
{
    using Newtonsoft.Json;

    public class TaskCandidateUser
    {
        public string userId;
        public string taskId;

        public TaskCandidateUser()
        {

        }

        [JsonConstructor]
        public TaskCandidateUser([JsonProperty("UserId")]string userId, [JsonProperty("TaskId")]string taskId)
        {
            this.userId = userId;
            this.taskId = taskId;
        }

        public virtual string UserId
        {
            get
            {
                return userId;
            }
        }

        public virtual string TaskId
        {
            get
            {
                return taskId;
            }
        }

    }

}