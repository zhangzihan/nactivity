using System.Text.RegularExpressions;

namespace Sys.Workflow.Options
{
    /// <summary>
    /// 数据库配置信息
    /// </summary>
    public sealed class DataSourceOption
    {
        private readonly static Regex regDatabase = new Regex("database=(\\w+);?", RegexOptions.IgnoreCase);

        private string connectionString;

        /// <summary>
        /// 数据库Provider类型名
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public string ConnectionString
        {
            get
            {

                if (string.IsNullOrWhiteSpace(Database))
                {
                    return connectionString;
                }

                if (ProviderName.ToLower() != "npgsql")
                {
                    if (new Regex("(Connection Timeout)", RegexOptions.IgnoreCase).IsMatch(connectionString) == false)
                    {
                        connectionString = string.Concat("Connection Timeout=300;", connectionString);
                    }
                }

                if (regDatabase.IsMatch(connectionString))
                {
                    return regDatabase.Replace(connectionString, match =>
                    {
                        return $"database={Database};";
                    });
                }
                else
                {
                    return $"database={Database};{connectionString}";
                }
            }
            set => connectionString = value;
        }

        /// <summary>
        /// 数据库
        /// </summary>
        public string Database { get; set; }

        public static bool operator ==(DataSourceOption a, DataSourceOption b)
        {
            if (a is null || b is null)
            {
                return false;
            }

            return a.ProviderName?.Trim() == b.ProviderName?.Trim() && a.ConnectionString.Trim() == b.ConnectionString.Trim();
        }

        public static bool operator !=(DataSourceOption a, DataSourceOption b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return this == obj as DataSourceOption;
        }

        public override int GetHashCode()
        {
            return ConnectionString.GetHashCode() << 2;
        }
    }
}
