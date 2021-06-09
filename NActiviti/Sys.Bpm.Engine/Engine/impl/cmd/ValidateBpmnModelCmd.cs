using System.Collections.Generic;

namespace Sys.Workflow.Engine.Impl.Cmd
{

    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Validation;

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
            if (processValidator is null)
            {
                throw new ActivitiException("No process validator defined");
            }

            return processValidator.Validate(bpmnModel);
        }

    }

}