// ==================================================================================
// Layered Architecture samples.
// Developed by Serena Yeoh - February 2021
// ==================================================================================
using Microsoft.Extensions.DependencyInjection;

namespace Sample.Data.Extensions
{
    public static class Register
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            // You can now register your data access components in this manner without polluting 
            // the business or service layer.
            services.AddTransient<ProductRepository>();

            return services;
        }
    }
}
