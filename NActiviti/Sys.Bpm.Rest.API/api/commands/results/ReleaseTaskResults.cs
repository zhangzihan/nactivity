using Newtonsoft.Json;

namespace org.activiti.cloud.services.api.commands.results
{

    /// <summary>
    /// 
    /// </summary>
    public class ReleaseTaskResults : ICommandResults
    {

        private readonly string id;
        private readonly string commandId;


        /// <summary>
        /// 
        /// </summary>
        public ReleaseTaskResults()
        {
            this.id = System.Guid.NewGuid().ToString();
        }


        /// <summary>
        /// 
        /// </summary>
        ////[JsonConstructor]
        public ReleaseTaskResults([JsonProperty("CommandId")]string commandId) : this()
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