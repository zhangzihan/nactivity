using System;
using System.Collections.Generic;
using System.Text;

namespace SmartSql.Configuration.Statements
{
    public class UpdateStatement : Statement
    {
        public override StatementType Type => StatementType.Update;
    }
}
