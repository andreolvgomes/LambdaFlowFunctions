using Microsoft.Extensions.DependencyInjection;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace LambdaFlowFunctions
{
    public interface IHandlerWithoutRequest
    {
        void Handler();
    }

    public interface IHandlerWithoutRequest<TResponse>
        where TResponse : class, new()
    {
        TResponse Handler();
    }

    public interface IHandler<TRequest>
        where TRequest : class, new()
    {
        void Handler(TRequest request);
    }

    public interface IHandler<TRequest, TResponse>
        where TResponse : class, new()
    {
        TResponse Handler(TRequest request);
    }

    public abstract class FunctionWithoutRequest<THandler> : FunctionBase
        where THandler : IHandlerWithoutRequest
    {
        public void Run(APIGatewayProxyRequest apiGatewayProxyRequest, ILambdaContext context)
        {
            using (var scope = CreateScope())
            {
                var func = scope.ServiceProvider.GetRequiredService<THandler>();
                func.Handler();
            }
        }
    }

    public abstract class FunctionWithoutRequest<THandler, TResponse> : FunctionBase
        where THandler : IHandlerWithoutRequest<TResponse>
        where TResponse : class, new()
    {
        public void Run(APIGatewayProxyRequest apiGatewayProxyRequest, ILambdaContext context)
        {
            using (var scope = CreateScope())
            {
                var func = scope.ServiceProvider.GetRequiredService<THandler>();
                var response = func.Handler();
            }
        }
    }

    public abstract class FunctionBase<THandler, TRequest> : FunctionBase
        where THandler : IHandler<TRequest>
            where TRequest : class, new()
    {
        public void Run(APIGatewayProxyRequest apiGatewayProxyRequest, ILambdaContext context)
        {
            using (var scope = CreateScope())
            {
                var request = Activator.CreateInstance<TRequest>();
                var func = scope.ServiceProvider.GetRequiredService<THandler>();
                func.Handler(request);
            }
        }
    }

    public abstract class Function<THandler, TRequest, TResponse> : FunctionBase
        where THandler : IHandler<TRequest, TResponse>
        where TRequest : class, new()
        where TResponse : class, new()
    {
        public async Task Run(APIGatewayProxyRequest apiGatewayProxyRequest, ILambdaContext context)
        {
            using (var scope = CreateScope())
            {
                //TResponse response = null;

                //await RunWithMiddleware(scope, apiGatewayProxyRequest, context, async () =>
                //{
                //    var request = Activator.CreateInstance<TRequest>();
                //    var func = scope.ServiceProvider.GetRequiredService<THandler>();
                //    response = func.Handler(request);
                //    await Task.CompletedTask;
                //});

                //return response;

                var request = Activator.CreateInstance<TRequest>();
                var func = scope.ServiceProvider.GetRequiredService<THandler>();
                var response = func.Handler(request);
            }
        }
    }

    public abstract class FunctionBase
    {
        protected readonly IServiceProvider _serviceProvider;

        public FunctionBase()
        {
            var services = IoC.Services();
            _serviceProvider = services.BuildServiceProvider();
        }

        protected IServiceScope CreateScope()
        {
            var scope = _serviceProvider.CreateScope();
            return scope;
        }

        protected async Task RunWithMiddleware(IServiceScope scope, APIGatewayProxyRequest apiGatewayProxyRequest, 
            ILambdaContext context, Func<Task> handlerExecution)
        {
            var pipeline = scope.ServiceProvider.GetRequiredService<MiddlewarePipeline>();
            await pipeline.ExecuteAsync(apiGatewayProxyRequest, context, handlerExecution);
        }
    }    
}