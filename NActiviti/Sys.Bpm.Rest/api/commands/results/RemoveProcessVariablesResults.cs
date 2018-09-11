using Newtonsoft.Json;

namespace org.activiti.cloud.services.api.commands.results
{
    public class RemoveProcessVariablesResults : CommandResults
    {

        private string id;
        private string commandId;


        public RemoveProcessVariablesResults()
        {
            this.id = System.Guid.NewGuid().ToString();
        }

        [JsonConstructor]
        public RemoveProcessVariablesResults([JsonProperty("CommandId")]string commandId) : this()
        {
            this.commandId = commandId;
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
    }
}