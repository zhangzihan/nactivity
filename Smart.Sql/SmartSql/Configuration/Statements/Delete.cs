using System;
using System.Collections.Generic;
using System.Text;

namespace SmartSql.Configuration.Statements
{
    public class DeleteStatement : Statement
    {
        public override StatementType Type => StatementType.Delete;
    }
}
