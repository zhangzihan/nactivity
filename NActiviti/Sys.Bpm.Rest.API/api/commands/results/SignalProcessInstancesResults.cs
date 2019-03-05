using Newtonsoft.Json;
using System;

namespace org.activiti.cloud.services.api.commands.results
{
    public class SignalProcessInstancesResults : ICommandResults
    {

        private string id;
        private string commandId;

        public SignalProcessInstancesResults()
        {
            this.id = Guid.NewGuid().ToString();
        }

        ////[JsonConstructor]
        public SignalProcessInstancesResults([JsonProperty("CommandId")]string commandId) : this()
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