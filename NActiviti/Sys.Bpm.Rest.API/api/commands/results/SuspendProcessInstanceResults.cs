using Newtonsoft.Json;
using System;

namespace org.activiti.cloud.services.api.commands.results
{
    public class SuspendProcessInstanceResults : ICommandResults
    {

        private string id;
        private string commandId;

        public SuspendProcessInstanceResults()
        {
            this.id = Guid.NewGuid().ToString();
        }

        ////[JsonConstructor]
        public SuspendProcessInstanceResults([JsonProperty("CommandId")]string commandId) : this()
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