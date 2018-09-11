namespace org.activiti.bpmn.converter.child
{

    using org.activiti.bpmn.model;

    public class ActivitiFailedjobRetryParser : BaseChildElementParser
    {

        public override string ElementName
        {
            get
            {
                return org.activiti.bpmn.constants.BpmnXMLConstants.FAILED_JOB_RETRY_TIME_CYCLE;
            }
        }
        public override void parseChildElement(XMLStreamReader xtr, BaseElement parentElement, BpmnModel model)
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