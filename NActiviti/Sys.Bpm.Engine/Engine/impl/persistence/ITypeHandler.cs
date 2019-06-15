using System;
using System.Data.Common;

namespace org.activiti.engine.impl.persistence
{
    public interface ITypeHandler<T>
    {
        T GetResult(DbDataReader rs, String columnName);

        T GetResult(DbDataReader rs, int columnIndex);
    }
}
