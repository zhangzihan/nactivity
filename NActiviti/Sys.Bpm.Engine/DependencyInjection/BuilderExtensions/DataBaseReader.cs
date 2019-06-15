using DatabaseSchemaReader;
using Microsoft.Extensions.DependencyInjection;
using Sys.Data;
using System.Data.Common;

namespace Sys.Workflow
{
    /// <summary>
    /// 注入DatabaseReader
    /// </summary>
    public static class ProcessEngineBuilderExtensionsDataBaseReader
    {
        /// <summary>
        /// 注入DatabaseReader
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IProcessEngineBuilder AddDataBaseReader(this IProcessEngineBuilder builder)
        {
            builder.Services.AddTransient<IDatabaseReader>(sp =>
            {
                var ds = sp.GetService<IDataSource>();

                DatabaseReader reader = new DatabaseReader(ds.Connection as DbConnection);

                switch (reader.DatabaseSchema.Provider)
                {
                    case "MySql.Data.MySqlClient":
                        reader.Owner = ds.Connection.Database;
                        break;
                }

                return reader;
            });

            return builder;
        }
    }
}
