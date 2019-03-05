using Newtonsoft.Json;

namespace org.activiti.cloud.services.api.model
{
    public class ProcessInstanceVariable
    {

        private string processInstanceId;

        private string name;

        private string type;

        private object value;

        private string executionId;

        public ProcessInstanceVariable()
        {

        }

        //[JsonConstructor]
        public ProcessInstanceVariable([JsonProperty("ProcessInstanceId")]string processInstanceId,
            [JsonProperty("Name")]string name,
            [JsonProperty("Type")]string type,
            [JsonProperty("Value")]object value,
            [JsonProperty("ExecutionId")]string executionId)
        {
            this.name = name;
            this.type = type;
            this.value = value;
            this.executionId = executionId;
            this.processInstanceId = processInstanceId;
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

        public virtual string ProcessInstanceId
        {
            get
            {
                return processInstanceId;
            }
        }

        public virtual string ExecutionId
        {
            get
            {
                return executionId;
            }
        }
    }

}