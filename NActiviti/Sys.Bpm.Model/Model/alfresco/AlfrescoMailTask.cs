namespace org.activiti.bpmn.model.alfresco
{

    public class AlfrescoMailTask : ServiceTask
    {

        public override BaseElement clone()
        {
            AlfrescoMailTask clone = new AlfrescoMailTask();
            clone.Values = this;
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