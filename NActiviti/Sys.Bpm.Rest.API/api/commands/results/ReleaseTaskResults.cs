using Newtonsoft.Json;

namespace org.activiti.cloud.services.api.commands.results
{
    public class ReleaseTaskResults : ICommandResults
    {

        private string id;
        private string commandId;

        public ReleaseTaskResults()
        {
            this.id = System.Guid.NewGuid().ToString();
        }

        ////[JsonConstructor]
        public ReleaseTaskResults([JsonProperty("CommandId")]string commandId) : this()
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