using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;

namespace LambdaFunctionFast
{
    public interface ILambdaMiddleware
    {
        Task<APIGatewayProxyResponse> InvokeAsync(APIGatewayProxyRequest request, ILambdaContext context, Func<Task<APIGatewayProxyResponse>> next);
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

        public async Task<APIGatewayProxyResponse> ExecuteAsync(APIGatewayProxyRequest request, ILambdaContext context)
        {
            Func<Task<APIGatewayProxyResponse>> pipeline = () => Task.FromResult<APIGatewayProxyResponse>(null);

            foreach (var middlewareType in _middlewareTypes)
            {
                var middleware = (ILambdaMiddleware)_serviceProvider.GetRequiredService(middlewareType);

                var next = pipeline;
                pipeline = async () => await middleware.InvokeAsync(request, context, next);
            }

            return await pipeline();
        }
    }
}