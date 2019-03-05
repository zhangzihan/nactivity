using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace org.activiti.cloud.services.api.commands
{
    public class RemoveProcessVariablesCmd : ICommand
    {
        private readonly string id = "removeProcessVariablesCmd";
        private string processId;
        private IList<string> variableNames;

        ////[JsonConstructor]
        public RemoveProcessVariablesCmd([JsonProperty("ProcessId")] string processId,
            [JsonProperty("Variables")]IList<string> variableNames)
        {
            this.processId = processId;
            this.variableNames = variableNames;
        }

        public virtual string Id
        {
            get => id;
        }

        public virtual string ProcessId
        {
            get => processId;
        }

        public virtual IList<string> VariableNames
        {
            get => variableNames;
        }
    }
}