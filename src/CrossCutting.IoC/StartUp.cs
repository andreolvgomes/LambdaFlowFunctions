using Infra.Repositories;
using LambdaFunctionFast;
using Microsoft.Extensions.DependencyInjection;

namespace CrossCutting.IoC
{
    public class StartUp
    {
        public static IServiceCollection ServiceCollection()
        {
            var services = new ServiceCollection();

            // Middlewares
            services.AddScoped<ApiKeyMiddleware>();
            services.AddScoped<LoggingMiddleware>();
            services.AddScoped<IRepository, Repository>();

            // Registrar pipeline
            services.AddSingleton(sp =>
            {
                return new MiddlewarePipeline(sp)
                    .Use<LoggingMiddleware>()
                    .Use<ApiKeyMiddleware>();
            });
            return services;

        }

        public static IServiceProvider ServiceProvider()
        {
            return ServiceCollection().BuildServiceProvider();
        }
    }
}