using System;
using System.Data.Common;

namespace Sys.Workflow.engine.impl.persistence
{
    public interface ITypeHandler<T>
    {
        T GetResult(DbDataReader rs, String columnName);

        T GetResult(DbDataReader rs, int columnIndex);
    }
}
