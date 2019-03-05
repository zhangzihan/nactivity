using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace org.activiti.cloud.services.api.commands
{
    public class SetProcessVariablesCmd : ICommand
    {

        private readonly string id = "setProcessVariablesCmd";
        private string processId;
        private IDictionary<string, object> variables;

        ////[JsonConstructor]
        public SetProcessVariablesCmd([JsonProperty("ProcessId")] string processId,
            [JsonProperty("Variables")] IDictionary<string, object> variables)
        {
            this.id = Guid.NewGuid().ToString();
            this.processId = processId;
            this.variables = variables;
        }

        public virtual string Id
        {
            get => id;
        }

        public virtual string ProcessId
        {
            get
            {
                return processId;
            }
        }

        public virtual IDictionary<string, object> Variables
        {
            get
            {
                return variables;
            }
        }
    }
}