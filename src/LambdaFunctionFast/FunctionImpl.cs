using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json.Serialization;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

namespace LambdaFunctionFast
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
        protected FunctionImpl(IServiceProvider serviceProvider) 
            : base(serviceProvider)
        {
        }

        public async Task<APIGatewayProxyResponse> Run(APIGatewayProxyRequest apiGateway, ILambdaContext context)
        {
            using (var scope = CreateScope())
            {
                var middlewareRespononse = await RunMiddleware(apiGateway, context);
                if (middlewareRespononse != null)
                    return middlewareRespononse;

                var request = DeserializeObject<TRequest>(apiGateway.Body);

                var errors = TryValidateObject(request);
                if (errors.Any())
                    return BadRequest(errors.Select(e => e.MemberNames.FirstOrDefault() + ": " + e.ErrorMessage).ToList());

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
        protected FunctionImpl(IServiceProvider serviceProvider) 
            : base(serviceProvider)
        {
        }

        public async Task<APIGatewayProxyResponse> Run(APIGatewayProxyRequest apiGateway, ILambdaContext context)
        {
            using (var scope = CreateScope())
            {
                var middlewareRespononse = await RunMiddleware(apiGateway, context);
                if (middlewareRespononse != null)
                    return middlewareRespononse;

                var request = DeserializeObject<TRequest>(apiGateway.Body);

                var errors = TryValidateObject(request);
                if (errors.Any())
                    return BadRequest(errors.Select(e => e.MemberNames.FirstOrDefault() + ": " + e.ErrorMessage).ToList());

                var func = scope.ServiceProvider.GetRequiredService<THandler>();
                var response = func.Handler(request, apiGateway, context);

                return Response(response);
            }
        }
    }

    public abstract class FunctionWithoutRequestImpl<THandler> : FunctionBase
        where THandler : IHandlerWithoutRequest
    {
        protected FunctionWithoutRequestImpl(IServiceProvider serviceProvider) 
            : base(serviceProvider)
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
        protected FunctionWithoutRequestImpl(IServiceProvider serviceProvider) 
            : base(serviceProvider)
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

        protected APIGatewayProxyResponse BadRequest(object response = null)
        {
            return new APIGatewayProxyResponse()
            {
            };
        }

        protected List<ValidationResult> TryValidateObject<T>(T obj)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(obj, null, null);
            Validator.TryValidateObject(obj, context, results, true);
            return results;
        }

        protected T DeserializeObject<T>(string value)
        {
            var camelSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            return JsonConvert.DeserializeObject<T>(value, settings: camelSettings);
        }
    }
}