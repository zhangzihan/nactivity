using Newtonsoft.Json;

namespace org.activiti.cloud.services.api.model
{
    public class TaskVariable
    {
        public enum TaskVariableScope
        {
            LOCAL,
            GLOBAL
        }

        private string taskId;

        private string name;

        private string type;

        private object value;

        private string executionId;

        private TaskVariableScope scope;

        public TaskVariable()
        {

        }

        [JsonConstructor]
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

        public virtual string Name
        {
            get
            {
                return name;
            }
        }


        public virtual string Type
        {
            get
            {
                return type;
            }
        }
        public virtual object Value
        {
            get
            {
                return value;
            }
        }

        public virtual string ExecutionId
        {
            get
            {
                return executionId;
            }
        }

        public virtual string TaskId
        {
            get
            {
                return taskId;
            }
        }

        public virtual TaskVariableScope Scope
        {
            get
            {
                return scope;
            }
        }
    }

}