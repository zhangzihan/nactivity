namespace org.activiti.cloud.services.api.commands
{
    using Newtonsoft.Json;

    public class SuspendProcessInstanceCmd : Command
    {

        private readonly string id;
        private string processInstanceId;

        [JsonConstructor]
        public SuspendProcessInstanceCmd([JsonProperty("ProcessInstanceId")]string processInstanceId)
        {
            this.id = System.Guid.NewGuid().ToString();
            this.processInstanceId = processInstanceId;
        }

        public virtual string Id
        {
            get
            {
                return id;
            }
        }

        public virtual string ProcessInstanceId
        {
            get
            {
                return processInstanceId;
            }
        }
    }

}