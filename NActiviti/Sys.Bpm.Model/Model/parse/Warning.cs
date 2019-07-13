namespace Sys.Workflow.Bpmn.Models.Parses
{

    public class Warning
    {

        protected internal string warningMessage;
        protected internal string resource;
        protected internal int line;
        protected internal int column;

        public Warning(string warningMessage, string localName, int lineNumber, int columnNumber)
        {
            this.warningMessage = warningMessage;
            this.resource = localName;
            this.line = lineNumber;
            this.column = columnNumber;
        }

        public Warning(string warningMessage, BaseElement element)
        {
            this.warningMessage = warningMessage;
            this.resource = element.Id;
            line = element.XmlRowNumber;
            column = element.XmlColumnNumber;
        }

        public override string ToString()
        {
            return warningMessage + (resource is object ? " | " + resource : "") + " | line " + line + " | column " + column;
        }
    }

}