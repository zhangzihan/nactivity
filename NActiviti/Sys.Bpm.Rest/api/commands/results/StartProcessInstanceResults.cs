using Newtonsoft.Json;
using org.activiti.cloud.services.api.model;

namespace org.activiti.cloud.services.api.commands.results
{
    public class StartProcessInstanceResults : CommandResults
    {

        private string id;
        private string commandId;

        private ProcessInstance processInstance;

        public StartProcessInstanceResults()
        {
            this.id = System.Guid.NewGuid().ToString();
        }

        ////[JsonConstructor]
        public StartProcessInstanceResults([JsonProperty("CommandId")]string commandId, [JsonProperty("ProcessInstance")]ProcessInstance processInstance) : this()
        {
            this.commandId = commandId;
            this.processInstance = processInstance;
        }

        public virtual string Id
        {
            get
            {
                return id;
            }
        }

        public virtual string CommandId
        {
            get
            {
                return commandId;
            }
        }

        public virtual ProcessInstance ProcessInstance
        {
            get
            {
                return processInstance;
            }
        }
    }

}