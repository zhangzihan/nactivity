namespace org.activiti.bpmn.model.alfresco
{

    public class AlfrescoScriptTask : ServiceTask
    {

        public const string ALFRESCO_SCRIPT_DELEGATE = "org.alfresco.repo.workflow.activiti.script.AlfrescoScriptDelegate";
        public const string ALFRESCO_SCRIPT_EXECUTION_LISTENER = "org.alfresco.repo.workflow.activiti.listener.ScriptExecutionListener";

        public override BaseElement clone()
        {
            AlfrescoScriptTask clone = new AlfrescoScriptTask();
            clone.Values = this;
            return clone;
        }
    }

}