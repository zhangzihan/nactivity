using Newtonsoft.Json;
using System;

namespace Sys.Workflow.cloud.services.api.commands.results
{

    /// <summary>
    /// 
    /// </summary>
    public class SuspendProcessInstanceResults : ICommandResults
    {

        private readonly string id;
        private readonly string commandId;


        /// <summary>
        /// 
        /// </summary>
        public SuspendProcessInstanceResults()
        {
            this.id = Guid.NewGuid().ToString();
        }


        /// <summary>
        /// 
        /// </summary>
        ////[JsonConstructor]
        public SuspendProcessInstanceResults([JsonProperty("CommandId")]string commandId) : this()
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