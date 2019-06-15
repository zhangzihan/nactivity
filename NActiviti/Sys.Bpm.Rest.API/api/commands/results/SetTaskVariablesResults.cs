using Newtonsoft.Json;

namespace org.activiti.cloud.services.api.commands.results
{

    /// <summary>
    /// 
    /// </summary>
    public class SetTaskVariablesResults : ICommandResults
    {

        private readonly string id;
        private readonly string commandId;



        /// <summary>
        /// 
        /// </summary>
        public SetTaskVariablesResults()
        {
            this.id = System.Guid.NewGuid().ToString();
        }

        /// <summary>
        /// 
        /// </summary>

        //[JsonConstructor()]
        public SetTaskVariablesResults([JsonProperty("CommandId")]string commandId) : this()
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