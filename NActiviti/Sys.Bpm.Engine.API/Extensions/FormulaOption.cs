using System;
using System.Collections.Generic;
using System.Text;

namespace Sys.Expressions
{
    public class FormulaOption
    {
        public string Alias { get; set; }

        public string Type { get; set; }

        public FormulaOption()
        {

        }

        public IList<FormulaOption> Formulas
        {
            get; set;
        }
    }
}
