using Newtonsoft.Json;
using Sys.Workflow.Cloud.Services.Api.Model;
using System;

namespace Sys.Workflow.Cloud.Services.Api.Commands.Results
{

    /// <summary>
    /// 
    /// </summary>
    public class StartProcessInstanceResults : ICommandResults
    {

        private readonly string id;
        private readonly string commandId;

        private readonly ProcessInstance processInstance;


        /// <summary>
        /// 
        /// </summary>
        public StartProcessInstanceResults()
        {
            this.id = Guid.NewGuid().ToString();
        }


        /// <summary>
        /// 
        /// </summary>
        ////[JsonConstructor]
        public StartProcessInstanceResults([JsonProperty("CommandId")]string commandId, [JsonProperty("ProcessInstance")]ProcessInstance processInstance) : this()
        {
            this.commandId = commandId;
            this.processInstance = processInstance;
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

        /// <summary>
        /// 
        /// </summary>

        public virtual ProcessInstance ProcessInstance
        {
            get
            {
                return processInstance;
            }
        }
    }

}