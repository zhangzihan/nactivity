using Newtonsoft.Json;
using System;

namespace Sys.Workflow.cloud.services.api.commands.results
{

    /// <summary>
    /// 
    /// </summary>
    public class SignalProcessInstancesResults : ICommandResults
    {

        private readonly string id;
        private readonly string commandId;


        /// <summary>
        /// 
        /// </summary>
        public SignalProcessInstancesResults()
        {
            this.id = Guid.NewGuid().ToString();
        }


        /// <summary>
        /// 
        /// </summary>
        ////[JsonConstructor]
        public SignalProcessInstancesResults([JsonProperty("CommandId")]string commandId) : this()
        {
            this.commandId = commandId;
        }

        /// <summary>
        /// 
        /// </summary>

        public virtual string Id
        {
            get
            {
                return id;
            }
        }

        /// <summary>
        /// 
        /// </summary>

        public virtual string CommandId
        {
            get
            {
                return commandId;
            }
        }
    }

}