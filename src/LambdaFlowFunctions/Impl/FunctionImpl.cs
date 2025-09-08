using Microsoft.Extensions.DependencyInjection;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace LambdaFlowFunctions.Impl
{
    public interface IHandler<TRequest>
        where TRequest : class, new()
    {
        void Handler(TRequest request, APIGatewayProxyRequest apiGateway, ILambdaContext context);
    }

    public interface IHandler<TRequest, TResponse>
        where TResponse : class, new()
    {
        TResponse Handler(TRequest request, APIGatewayProxyRequest apiGateway, ILambdaContext context);
    }

    public interface IHandlerWithoutRequest
    {
        void Handler(APIGatewayProxyRequest apiGateway, ILambdaContext context);
    }

    public interface IHandlerWithoutRequest<TResponse>
        where TResponse : class, new()
    {
        TResponse Handler(APIGatewayProxyRequest apiGateway, ILambdaContext context);
    }

    public abstract class FunctionImpl<THandler, TRequest> : FunctionBase
        where THandler : IHandler<TRequest>
        where TRequest : class, new()
    {
        protected FunctionImpl(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public async Task<APIGatewayProxyResponse> Run(APIGatewayProxyRequest apiGateway, ILambdaContext context)
        {
            using (var scope = CreateScope())
            {
                var middlewareRespononse = await RunMiddleware(apiGateway, context);
                if (middlewareRespononse != null)
                    return middlewareRespononse;

                var request = Activator.CreateInstance<TRequest>();
                var func = scope.ServiceProvider.GetRequiredService<THandler>();
                func.Handler(request, apiGateway, context);

                return Response();
            }
        }
    }

    public abstract class FunctionImpl<THandler, TRequest, TResponse> : FunctionBase
        where THandler : IHandler<TRequest, TResponse>
        where TRequest : class, new()
        where TResponse : class, new()
    {
        protected FunctionImpl(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public async Task<APIGatewayProxyResponse> Run(APIGatewayProxyRequest apiGateway, ILambdaContext context)
        {
            using (var scope = CreateScope())
            {
                var middlewareRespononse = await RunMiddleware(apiGateway, context);
                if (middlewareRespononse != null)
                    return middlewareRespononse;

                var request = Activator.CreateInstance<TRequest>();
                var func = scope.ServiceProvider.GetRequiredService<THandler>();
                var response = func.Handler(request, apiGateway, context);

                return Response(response);
            }
        }
    }

    public abstract class FunctionWithoutRequestImpl<THandler> : FunctionBase
        where THandler : IHandlerWithoutRequest
    {
        protected FunctionWithoutRequestImpl(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public async Task<APIGatewayProxyResponse> Run(APIGatewayProxyRequest apiGateway, ILambdaContext context)
        {
            using (var scope = CreateScope())
            {
                var middlewareRespononse = await RunMiddleware(apiGateway, context);
                if (middlewareRespononse != null)
                    return middlewareRespononse;

                var func = scope.ServiceProvider.GetRequiredService<THandler>();
                func.Handler(apiGateway, context);

                return Response();
            }
        }
    }

    public abstract class FunctionWithoutRequestImpl<THandler, TResponse> : FunctionBase
        where THandler : IHandlerWithoutRequest<TResponse>
        where TResponse : class, new()
    {
        protected FunctionWithoutRequestImpl(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public async Task<APIGatewayProxyResponse> Run(APIGatewayProxyRequest apiGateway, ILambdaContext context)
        {
            using (var scope = CreateScope())
            {
                var middlewareRespononse = await RunMiddleware(apiGateway, context);
                if (middlewareRespononse != null)
                    return middlewareRespononse;

                var func = scope.ServiceProvider.GetRequiredService<THandler>();
                var response = func.Handler(apiGateway, context);

                return Response(response);
            }
        }
    }

    public abstract class FunctionBase
    {
        protected readonly IServiceProvider _serviceProvider;

        public FunctionBase(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected IServiceScope CreateScope()
        {
            var scope = _serviceProvider.CreateScope();
            return scope;
        }

        protected async Task<APIGatewayProxyResponse> RunMiddleware(APIGatewayProxyRequest apiGateway, ILambdaContext context)
        {
            var pipeline = _serviceProvider.GetRequiredService<MiddlewarePipeline>();
            return await pipeline.ExecuteAsync(apiGateway, context);
        }

        protected APIGatewayProxyResponse Response(object response = null)
        {
            return new APIGatewayProxyResponse()
            {
            };
        }
    }
}