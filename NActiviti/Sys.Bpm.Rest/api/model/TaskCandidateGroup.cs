using Newtonsoft.Json;

namespace org.activiti.cloud.services.api.model
{
    public class TaskCandidateGroup
    {
        public string groupId;
        public string taskId;

        public TaskCandidateGroup()
        {

        }

        [JsonConstructor]
        public TaskCandidateGroup([JsonProperty("GroupId")]string groupId, [JsonProperty("TaskId")]string taskId)
        {
            this.groupId = groupId;
            this.taskId = taskId;
        }


        public virtual string GroupId
        {
            get
            {
                return groupId;
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