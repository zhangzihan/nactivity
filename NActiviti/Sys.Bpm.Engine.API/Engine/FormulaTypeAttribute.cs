using System;
using System.Collections.Generic;
using System.Text;

namespace Sys.Workflow.Expressions
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class FormulaTypeAttribute : Attribute
    {
        public string Type
        {
            get;
            private set;
        }

        public FormulaTypeAttribute(string type)
        {
            Type = type;
        }
    }
}
