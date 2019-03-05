namespace org.activiti.cloud.services.api.commands
{
    using Newtonsoft.Json;
    using System;

    public class SuspendProcessInstanceCmd : ICommand
    {

        private readonly string id = "suspendProcessInstanceCmd";
        private string processInstanceId;

        //[JsonConstructor]
        public SuspendProcessInstanceCmd([JsonProperty("ProcessInstanceId")]string processInstanceId)
        {
            this.processInstanceId = processInstanceId;
        }

        public virtual string Id
        {
            get => id;
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