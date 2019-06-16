namespace Sys.Workflow.Bpmn.Models.Alfresco
{

    public class AlfrescoUserTask : UserTask
    {

        public const string ALFRESCO_SCRIPT_TASK_LISTENER = "org.alfresco.repo.workflow.activiti.tasklistener.ScriptTaskListener";

        protected internal string runAs;
        protected internal string scriptProcessor;

        public virtual string RunAs
        {
            get
            {
                return runAs;
            }
            set
            {
                this.runAs = value;
            }
        }


        public virtual string ScriptProcessor
        {
            get
            {
                return scriptProcessor;
            }
            set
            {
                this.scriptProcessor = value;
            }
        }


        public override BaseElement Clone()
        {
            AlfrescoUserTask clone = new AlfrescoUserTask
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
                var val = value as AlfrescoUserTask;

                RunAs = val.RunAs;
                ScriptProcessor = val.ScriptProcessor;
            }
        }
    }

}