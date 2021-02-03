// ==================================================================================
// Layered Architecture samples.
// Developed by Serena Yeoh - February 2021
// ==================================================================================
using Microsoft.Extensions.DependencyInjection;
using Sample.Data.Extensions;

namespace Sample.Business.Extensions
{
    public static class Register
    {
        public static IServiceCollection AddDomainComponents(this IServiceCollection services)
        {
            // You can now register your business components in this manner without polluting 
            // the service layer.
            services.AddTransient<ProductComponent>();
            
            // This calls the data layer extension method to register the data components.
            services.AddRepositories();

            return services;
        }
    }
}
