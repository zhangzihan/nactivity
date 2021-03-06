﻿namespace Sys.Workflow.Bpmn.Models.Alfresco
{

    public class AlfrescoStartEvent : StartEvent
    {

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
            AlfrescoStartEvent clone = new AlfrescoStartEvent
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

                var val = value as AlfrescoStartEvent;
                RunAs = val.RunAs;
                ScriptProcessor = val.ScriptProcessor;
            }
        }
    }

}