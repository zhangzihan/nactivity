using Newtonsoft.Json;
using System;

namespace org.activiti.cloud.services.api.commands
{
    public class ActivateProcessInstanceCmd : ICommand
    {

        private readonly string id = "activateProcessInstanceCmd";

        private string processInstanceId;

        ////[JsonConstructor]
        public ActivateProcessInstanceCmd([JsonProperty("ProcessInstanceId")]string processInstanceId)
        {
            this.processInstanceId = processInstanceId;
        }

        public virtual string Id
        {
            get
            {
                return id;
            }
        }

        public virtual string ProcessInstanceId
        {
            get
            {
                return processInstanceId;
            }
        }
    }

}