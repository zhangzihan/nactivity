using Newtonsoft.Json;

namespace org.activiti.cloud.services.api.commands.results
{

    /// <summary>
    /// 
    /// </summary>
    public class ClaimTaskResults : ICommandResults
    {


        private readonly string id;
        private readonly string commandId;


        /// <summary>
        /// 
        /// </summary>
        public ClaimTaskResults()
        {
            this.id = System.Guid.NewGuid().ToString();
        }


        /// <summary>
        /// 
        /// </summary>
        ////[JsonConstructor]
        public ClaimTaskResults([JsonProperty("CommandId")]string commandId) : this()
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