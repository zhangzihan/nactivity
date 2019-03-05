using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Data
{
    public interface IDataSource
    {
        DbProviderFactory DbProviderFactory {get;}

        string ConnectionString { get; }

        IDbConnection Connection { get; }

        void forceCloseAll();

        IDbConnection getConnection();
    }
}
