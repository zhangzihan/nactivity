namespace Sys.Workflow.Bpmn.Converters.Childs
{
    using Sys.Workflow.Bpmn.Constants;
    using Sys.Workflow.Bpmn.Models;

    public class ActivitiFailedjobRetryParser : BaseChildElementParser
    {

        public override string ElementName
        {
            get
            {
                return BpmnXMLConstants.FAILED_JOB_RETRY_TIME_CYCLE;
            }
        }
        public override void ParseChildElement(XMLStreamReader xtr, BaseElement parentElement, BpmnModel model)
        {
            if (!(parentElement is Activity))
            {
                return;
            }
            string cycle = xtr.ElementText;
            if (string.ReferenceEquals(cycle, null) || cycle.Length == 0)
            {
                return;
            }
          ((Activity)parentElement).FailedJobRetryTimeCycleValue = cycle;
        }

    }

}