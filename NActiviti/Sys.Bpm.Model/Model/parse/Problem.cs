namespace Sys.Workflow.Bpmn.Models.Parses
{

    public class Problem
    {

        protected internal string errorMessage;
        protected internal string resource;
        protected internal int line;
        protected internal int column;

        public Problem(string errorMessage, string localName, int lineNumber, int columnNumber)
        {
            this.errorMessage = errorMessage;
            this.resource = localName;
            this.line = lineNumber;
            this.column = columnNumber;
        }

        public Problem(string errorMessage, BaseElement element)
        {
            this.errorMessage = errorMessage;
            this.resource = element.Id;
            this.line = element.XmlRowNumber;
            this.column = element.XmlColumnNumber;
        }

        public Problem(string errorMessage, GraphicInfo graphicInfo)
        {
            this.errorMessage = errorMessage;
            this.line = graphicInfo.XmlRowNumber;
            this.column = graphicInfo.XmlColumnNumber;
        }

        public override string ToString()
        {
            return errorMessage + (resource is object ? " | " + resource : "") + " | line " + line + " | column " + column;
        }
    }

}