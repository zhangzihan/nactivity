using Microsoft.Extensions.DependencyInjection;
using Sys.Expressions;

namespace Sys.Workflow
{
    public static class SpringCoreExtentions
    {
        public static void AddSpringCoreTypeRepository(this IServiceCollection services)
        {
            services.Configure<FormulaOption>(formulaOption =>
            {
            });

            var registry = new ExpressionTypeRegistry();

            services.AddSingleton(registry);
        }
    }
}
