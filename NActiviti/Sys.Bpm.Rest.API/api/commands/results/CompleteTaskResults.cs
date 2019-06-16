using Newtonsoft.Json;

namespace Sys.Workflow.Cloud.Services.Api.Commands.Results
{

    /// <summary>
    /// 
    /// </summary>
    public class CompleteTaskResults : ICommandResults
    {

        private readonly string id;
        private readonly string commandId;


        /// <summary>
        /// 
        /// </summary>
        public CompleteTaskResults()
        {
            this.id = System.Guid.NewGuid().ToString();
        }


        /// <summary>
        /// 
        /// </summary>
        ////[JsonConstructor]
        public CompleteTaskResults([JsonProperty("CommandId")]string commandId) : this()
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