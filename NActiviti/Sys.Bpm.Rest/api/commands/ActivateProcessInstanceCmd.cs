using Newtonsoft.Json;

namespace org.activiti.cloud.services.api.commands
{
    public class ActivateProcessInstanceCmd : Command
    {

        private readonly string id;

        private string processInstanceId;

        ////[JsonConstructor]
        public ActivateProcessInstanceCmd([JsonProperty("ProcessInstanceId")]string processInstanceId)
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