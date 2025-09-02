using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;

namespace LambdaFlowFunctions
{
    public interface ILambdaMiddleware
    {
        Task InvokeAsync(APIGatewayProxyRequest request, ILambdaContext context, Func<Task> next);
    }

    public class MiddlewarePipeline
    {
        private readonly IList<Type> _middlewareTypes = new List<Type>();
        private readonly IServiceProvider _serviceProvider;

        public MiddlewarePipeline(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public MiddlewarePipeline Use<TMiddleware>() where TMiddleware : ILambdaMiddleware
        {
            _middlewareTypes.Add(typeof(TMiddleware));
            return this;
        }

        public async Task ExecuteAsync(APIGatewayProxyRequest request, ILambdaContext context, Func<Task> finalHandler)
        {
            var index = 0;

            async Task Next()
            {
                if (index < _middlewareTypes.Count)
                {
                    var middlewareType = _middlewareTypes[index++];
                    var middleware = (ILambdaMiddleware)_serviceProvider.GetRequiredService(middlewareType);
                    await middleware.InvokeAsync(request, context, Next);
                }
                else
                {
                    await finalHandler();
                }
            }

            await Next();
        }
    }

    public class ApiKeyMiddleware : ILambdaMiddleware
    {
        public async Task InvokeAsync(APIGatewayProxyRequest request, ILambdaContext context, Func<Task> next)
        {
            if (!request.Headers.TryGetValue("x-api-key", out var apiKey) || apiKey != "minha-chave")
            {
                throw new UnauthorizedAccessException("API Key inválida!");
            }

            await next();
        }
    }

    public class LoggingMiddleware : ILambdaMiddleware
    {
        public async Task InvokeAsync(APIGatewayProxyRequest request, ILambdaContext context, Func<Task> next)
        {
            Console.WriteLine($"[Request] {request.HttpMethod} {request.Path}");
            await next();
            Console.WriteLine("[Response] concluído");
        }
    }
}
