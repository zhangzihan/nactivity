using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Data
{
    public interface IEntityManager
    {
        IDbTransaction Transaction { get; set; }

        bool Open { get; set; }

        void Close();

        object Find(Type entityClass, object primaryKey);

        void Flush();
    }
}
