using System.Collections.Generic;

namespace org.activiti.engine.impl.cmd
{

    using org.activiti.bpmn.model;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.validation;

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