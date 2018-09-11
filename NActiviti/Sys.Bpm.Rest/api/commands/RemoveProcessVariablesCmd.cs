using Newtonsoft.Json;
using System.Collections.Generic;

namespace org.activiti.cloud.services.api.commands
{
    public class RemoveProcessVariablesCmd : Command
    {

        private readonly string id;
        private string processId;
        private IList<string> variableNames;

        [JsonConstructor]
        public RemoveProcessVariablesCmd([JsonProperty("ProcessId")] string processId,
            [JsonProperty("Variables")]IList<string> variableNames)
        {
            this.id = System.Guid.NewGuid().ToString();
            this.processId = processId;
            this.variableNames = variableNames;
        }

        public virtual string Id
        {
            get
            {
                return id;
            }
        }

        public virtual string ProcessId
        {
            get
            {
                return processId;
            }
        }

        public virtual IList<string> VariableNames
        {
            get
            {
                return variableNames;
            }
        }
    }
}