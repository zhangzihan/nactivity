﻿using Newtonsoft.Json;

namespace Sys.Workflow.Cloud.Services.Api.Commands.Results
{

    /// <summary>
    /// 
    /// </summary>
    public class SetProcessVariablesResults : ICommandResults
    {

        private readonly string id;
        private readonly string commandId;



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