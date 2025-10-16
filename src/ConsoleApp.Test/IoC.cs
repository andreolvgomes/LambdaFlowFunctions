using LambdaFunctionFast;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ConsoleApp.Test
{
    public static class IoC
    {
        public static IServiceProvider Services()
        {
            var services = new ServiceCollection();
            services.AddScoped<ProdutosHandlerWithoutRequest2>();
            services.AddScoped<ProdutosHandlerWithoutRequest>();
            services.AddScoped<ProdutosHandler>();
            services.AddScoped<ProdutosHandler2>();

            // Middlewares
            services.AddScoped<ApiKeyMiddleware>();
            services.AddScoped<LoggingMiddleware>();

            // Registrar pipeline
            services.AddSingleton(sp =>
            {
                return new MiddlewarePipeline(sp)
                    .Use<LoggingMiddleware>()
                    .Use<ApiKeyMiddleware>();
            });
            return services.BuildServiceProvider();
        }
    }
}