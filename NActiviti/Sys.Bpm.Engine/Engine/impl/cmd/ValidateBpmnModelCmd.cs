using System.Collections.Generic;

namespace Sys.Workflow.engine.impl.cmd
{

    using Sys.Workflow.bpmn.model;
    using Sys.Workflow.engine.impl.interceptor;
    using Sys.Workflow.validation;

    /// 
    public class ValidateBpmnModelCmd : ICommand<IList<ValidationError>>
    {

        protected internal BpmnModel bpmnModel;

        public ValidateBpmnModelCmd(BpmnModel bpmnModel)
        {
            this.bpmnModel = bpmnModel;
        }

        public virtual IList<ValidationError> Execute(ICommandContext commandContext)
        {
            IProcessValidator processValidator = commandContext.ProcessEngineConfiguration.ProcessValidator;
            if (processValidator == null)
            {
                throw new ActivitiException("No process validator defined");
            }

            return processValidator.Validate(bpmnModel);
        }

    }

}