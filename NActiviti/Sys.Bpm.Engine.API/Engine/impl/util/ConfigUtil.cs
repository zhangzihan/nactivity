using Microsoft.Extensions.Configuration;

namespace Sys.Workflow.Util
{
    public class ConfigUtil
    {
        public static IConfiguration Configuration { get; set; }

        public static string Value(string key)
        {
            return Configuration.GetValue<string>(key)?.ToString();
        }
    }
}
