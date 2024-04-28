using SecuredAuthApp.Data;

namespace SecuredAuthApp.IOC
{
    public static class DependencyContainer
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services) 
        {
            services.AddScoped<AppDbContext>();
            return services;
        }
    }
}
