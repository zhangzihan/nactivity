using System;
using System.Collections.Generic;
using System.Text;

namespace org.activiti.cloud.services.api.model
{
    public class ProcessVariablesQuery
    {
        public string Id { get; set; }

        public string ProcessInstanceId { get; set; }

        public string TaskId { get; set; }

        public string VariableName { get; set; }

        public bool ExcludeTaskVariables { get; set; }
    }
}
