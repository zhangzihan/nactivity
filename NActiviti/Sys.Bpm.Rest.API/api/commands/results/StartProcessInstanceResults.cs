using Newtonsoft.Json;
using org.activiti.cloud.services.api.model;
using System;

namespace org.activiti.cloud.services.api.commands.results
{

    /// <summary>
    /// 
    /// </summary>
    public class StartProcessInstanceResults : ICommandResults
    {

        private string id;
        private string commandId;

        private ProcessInstance processInstance;


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