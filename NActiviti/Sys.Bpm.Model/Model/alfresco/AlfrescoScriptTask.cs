namespace Sys.Workflow.Bpmn.Models.Alfresco
{

    public class AlfrescoScriptTask : ServiceTask
    {

        public const string ALFRESCO_SCRIPT_DELEGATE = "org.alfresco.repo.workflow.activiti.script.AlfrescoScriptDelegate";
        public const string ALFRESCO_SCRIPT_EXECUTION_LISTENER = "org.alfresco.repo.workflow.activiti.listener.ScriptExecutionListener";

        public override BaseElement Clone()
        {
            AlfrescoScriptTask clone = new AlfrescoScriptTask
            {
                Values = this
            };
            return clone;
        }
    }

}