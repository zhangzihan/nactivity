namespace Sys.Workflow.Bpmn.Models.Alfresco
{

    public class AlfrescoMailTask : ServiceTask
    {

        public override BaseElement Clone()
        {
            AlfrescoMailTask clone = new AlfrescoMailTask
            {
                Values = this
            };
            return clone;
        }

        public override BaseElement Values
        {
            set
            {
                base.Values = value;
            }
        }
    }

}