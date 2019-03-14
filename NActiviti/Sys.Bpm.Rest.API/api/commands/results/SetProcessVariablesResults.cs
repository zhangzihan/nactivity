using Newtonsoft.Json;

namespace org.activiti.cloud.services.api.commands.results
{

    /// <summary>
    /// 
    /// </summary>
    public class SetProcessVariablesResults : ICommandResults
    {

        private string id;
        private string commandId;



        /// <summary>
        /// 
        /// </summary>
        public SetProcessVariablesResults()
        {
            this.id = System.Guid.NewGuid().ToString();
        }

        /// <summary>
        /// 
        /// </summary>

        ////[JsonConstructor]
        public SetProcessVariablesResults([JsonProperty("CommandId")]string commandId) : this()
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