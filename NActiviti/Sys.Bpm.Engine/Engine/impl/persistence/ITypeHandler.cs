using System;
using System.Data.Common;

namespace org.activiti.engine.impl.persistence
{
    public interface ITypeHandler<T>
    {
        T getResult(DbDataReader rs, String columnName);

        T getResult(DbDataReader rs, int columnIndex);
    }
}
