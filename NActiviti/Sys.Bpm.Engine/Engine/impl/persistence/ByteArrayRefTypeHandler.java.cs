namespace org.activiti.engine.impl.persistence
{
    using org.activiti.engine.impl.persistence.entity;
    using System;
    using System.Data.Common;

    /// <summary>
    /// MyBatis TypeHandler for <seealso cref="ByteArrayRef"/>.
    /// 
    /// 
    /// </summary>
    public class ByteArrayRefTypeHandler : TypeReference<ByteArrayRef>, ITypeHandler<ByteArrayRef>
    {
        public virtual void setParameter(DbCommand ps, int i, ByteArrayRef parameter)
        {
            ps.Parameters[i].Value = getValueToSet(parameter);
        }

        private string getValueToSet(ByteArrayRef parameter)
        {
            if (parameter == null)
            {
                // Note that this should not happen: ByteArrayRefs should always be initialized.
                return null;
            }
            return parameter.Id;
        }

        public virtual ByteArrayRef getResult(DbDataReader rs, string columnName)
        {
            return getResult(rs, rs.GetOrdinal(columnName));
        }

        public virtual ByteArrayRef getResult(DbDataReader rs, int columnIndex)
        {
            string id = rs.GetString(columnIndex);
            return new ByteArrayRef(id);
        }
    }
}